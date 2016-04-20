using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;
using CyPhy = ISIS.GME.Dsml.CyPhyML.Interfaces;
using CyPhyClasses = ISIS.GME.Dsml.CyPhyML.Classes;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;


namespace CyPhy2SystemC.SystemC
{
    public class CodeGenerator
    {
        public static GME.CSharp.GMEConsole GMEConsole { get; set; }
        public static bool verbose { get; set; }
        public static string BasePath { get; set; } // Path of the root testbench object in GME object tree

        static CodeGenerator()
        {
            verbose = false;
        }

        private CyPhyGUIs.IInterpreterMainParameters mainParameters { get; set; }

        public CodeGenerator(CyPhyGUIs.IInterpreterMainParameters parameters)
        {
            this.mainParameters = parameters;
            CodeGenerator.verbose = ((CyPhy2SystemC.CyPhy2SystemC_Settings)parameters.config).Verbose;
        }

        public void GenerateSystemCCode()
        {
            // map the root testbench obj
            var testbench = CyPhyClasses.TestBench.Cast(this.mainParameters.CurrentFCO);
            if (testbench == null)
            {
                GMEConsole.Error.WriteLine("Invalid context of invocation <{0}>, invoke the interpreter from a Testbench model", 
                    this.mainParameters.CurrentFCO.Name);
                return;
            }
            var TestBench_obj = new TestBench(testbench);
            BasePath = testbench.Path;

            // Notes: The interepreter works in two (and half) traversal passes
            // 1. A first traversal maps CyPhy objects to a corresponding but significantly lighter weight object network that only includes a 
            //     small set of concepts/classes : TestBench, ComponentAssembly, Component, Parameter, Port, Connection
            //     This traversal first builds the object network and then wires it up 
            //      the object network is hierarchical, but the wiring is direct and skips hierarchy. The dependency on CyPhy is largely localized to the 
            //      traversal/visitor code (CyPhyVisitors.cs)
            TestBench_obj.accept(new CyPhyBuildVisitor(this.mainParameters.ProjectDirectory));
            TestBench_obj.accept(new CyPhyConnectVisitor());

            // 2. The second traversal walks the lighter weight (largely CyPhy independent) object network and maps to the eagle XML object network
            //      the classes of this object network are automatically derived from the eagle XSD using the XSD2Code tool in the META repo
            //      an important step of this traversal is the routing which is implemented currently as a simple rats nest routing, but could be improved later
            //        the traversal and visitor code is localized in (SystemCTraversal.cs)
            var systemCVisitor = new SystemCVisitor(this.mainParameters.OutputDirectory);
            TestBench_obj.accept(systemCVisitor);

            // 2.5  Finally generate the source file (and project) based on the object graph

            // Unpack SystemC skeleton project and libraries
            try
            {
                var thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.Stream zipStream =
                    thisAssembly.GetManifestResourceStream("CyPhy2SystemC.SystemCTestBench.zip");
                ZipFile zip = ZipFile.Read(zipStream);
                zip.ExtractAll(this.mainParameters.OutputDirectory);

                string vcProjPath = Path.Combine(this.mainParameters.OutputDirectory, "SystemCTestBench.vcxproj");
                var vcProj = new Project(vcProjPath);

                // Find and copy additional sources into the project directory
                // Also add sources to the Visual Studio project file
                foreach (var source in systemCVisitor.Sources) {
                    if (source.Type == SourceFile.SourceType.Header) {
                        string incPath = Path.Combine("include", "TonkaSCLib", source.Path);
                        if (File.Exists(Path.Combine(this.mainParameters.OutputDirectory, source.Path)))
                        {
                            vcProj.AddItem("ClInclude", source.Path);
                        }
                        else if (File.Exists(Path.Combine(this.mainParameters.OutputDirectory, incPath))) {
                            vcProj.AddItem("ClInclude", incPath);
                        }
                        else if (File.Exists(Path.Combine(this.mainParameters.ProjectDirectory, source.Path)))
                        {
                            incPath = Path.Combine("include", source.Path);
                            string srcPath = Path.Combine(this.mainParameters.ProjectDirectory, source.Path);
                            string dstPath = Path.Combine(this.mainParameters.OutputDirectory, incPath);
                            string dstDir = Path.GetDirectoryName(dstPath);
                            if (!Directory.Exists(dstDir))
                            {
                                Directory.CreateDirectory(dstDir);
                            }
                            File.Copy(srcPath, dstPath);


                            vcProj.AddItem("ClInclude", incPath);
                        }
                        else
                        {
                            GMEConsole.Error.WriteLine("Unable to find SystemC header file: " + source.Path);
                        }
                    }

                    if (source.Type == SourceFile.SourceType.Implemnation ||
                        source.Type == SourceFile.SourceType.Arduino)
                    {
                        Action addItem = delegate ()
                        {
                            if (source.Type == SourceFile.SourceType.Implemnation)
                            {
                                vcProj.AddItem("ClCompile", source.Path);
                            }
                            if (source.Type == SourceFile.SourceType.Arduino)
                            {
                                vcProj.AddItem("Arduino", source.Path);
                            }
                        };

                        string srcPath = Path.Combine(this.mainParameters.ProjectDirectory, source.Path);
                        string dstPath = Path.Combine(this.mainParameters.OutputDirectory, source.Path);
                        if (File.Exists(Path.Combine(this.mainParameters.OutputDirectory, dstPath)))
                        {
                            addItem();
                        }
                        else if (File.Exists(srcPath))
                        {
                            string dstDir = Path.GetDirectoryName(dstPath);
                            if (!Directory.Exists(dstDir))
                            {
                                Directory.CreateDirectory(dstDir);
                            }
                            File.Copy(srcPath, dstPath);

                            addItem();
                        }
                        else
                        {
                            GMEConsole.Error.WriteLine("Unable to find SystemC source file: " + source.Path);
                        }
                    }
                }

                vcProj.Save(vcProjPath);
                vcProj = null;

            }
            catch (Exception e)
            {
                GMEConsole.Error.WriteLine("Unable to extract SystemC libraries and template project: " + e.Message + "<br>Inner: " + e.InnerException + "<br>Stack: " + e.StackTrace);
            }


            System.IO.Directory.CreateDirectory(this.mainParameters.OutputDirectory);
            String outFile = this.mainParameters.OutputDirectory + "/test_main.cpp";
            try
            {
                systemCVisitor.WriteFile(outFile);
            }
            catch (Exception ex)
            {
                GMEConsole.Error.WriteLine("Error generating SystemC source code: " + outFile + "<br> Exception: " + ex.Message + "<br> Trace: " + ex.StackTrace);
            }
        }

    }
 

 

}
