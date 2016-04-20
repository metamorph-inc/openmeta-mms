using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using GME.MGA;
using GME.CSharp;
using META;
using CyPhyGUIs;

using CyPhyMasterInterpreter;

namespace CyPhy2PCBMfgTest
{
    public class InterpreterFixture : IDisposable
    {
        public static String path_Test = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                      "..\\..\\..\\..",
                                                      "test",
                                                      "CyPhy2PCBMfgTest",
                                                      "model");

        private static String path_XME = Path.Combine(path_Test,
                                                      "pcbMfgTest.xme");

        public MgaProject proj { get; private set; }

        public InterpreterFixture()
        {
            String mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(path_XME, out mgaConnectionString);
            var path_MGA = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(path_MGA)),
                        String.Format("{0} not found. Model import may have failed.", path_MGA));

            if (Directory.Exists(Path.Combine(path_Test, "output")))
            {
                Directory.Delete(Path.Combine(path_Test, "output"), true);
            }

            proj = new MgaProject();
            bool ro_mode;
            proj.Open(mgaConnectionString, out ro_mode);
            proj.EnableAutoAddOns(true);
        }

        public void Dispose()
        {
            proj.Save();
            proj.Close();
            proj = null;
        }
    }

    public class InterpreterTest : IUseFixture<InterpreterFixture>
    {
        #region Fixture
        InterpreterFixture fixture;
        public void SetFixture(InterpreterFixture data)
        {
            fixture = data;
        }
        #endregion

        private MgaProject project
        {
            get
            {
                return fixture.proj;
            }
        }

        private String TestPath
        {
            get
            {
                return InterpreterFixture.path_Test;
            }
        }

        [Fact]
        public void PCB_Manufacturing()
        {
            /* What do we want to do?
             * Let's load the model and run the test bench.
             * Then check the output folder.
             * Verify that it contains the stuff that we expect.
             */

            var pathTestbench = "/@TestBenches|kind=Testing|relpos=0/PCB_Manufacturing|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);

            if (Directory.Exists(pathOutput))
            {
                Directory.Delete(pathOutput);
            }
            Directory.CreateDirectory(pathOutput);

            MgaFCO objTestBench = null;
            project.PerformInTransaction(delegate
            {
                objTestBench = project.get_ObjectByPath(pathTestbench) as MgaFCO;
                Assert.NotNull(objTestBench);
            });

            var interpreter = new CyPhy2PCBMfg.CyPhy2PCBMfgInterpreter();
            interpreter.Initialize(project);
            var parameters = new InterpreterMainParameters()
            {
                CurrentFCO = objTestBench,
                SelectedFCOs = null,
                Project = project,
                OutputDirectory = pathOutput,
                ProjectDirectory = project.GetRootDirectoryPath()
            };
            interpreter.Main(parameters);

            // Check that files were copied from the design path to the output path.
            string[] copiedFileNames = 
            {
                "BomTable.csv",
                "pcb_mfg.cam",
                "reference_designator_mapping_table.html",
                "schema.brd",
                "schema.sch"
            };

            foreach (string filename in copiedFileNames)
            {
                var pathCopiedFile = Path.Combine(pathOutput, filename);
                Assert.True(File.Exists(pathCopiedFile));
                var fileContents = File.ReadAllText(pathCopiedFile);
                Assert.False(String.IsNullOrWhiteSpace(fileContents));
            }
        }
        [Fact]
        public void PCBMfg_WorkflowTests()
        {
            var pathTestbench = "/@TestBenches|kind=Testing|relpos=0/PCB_Manufacturing|kind=TestBench|relpos=0";
            String testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String pathOutput = Path.Combine(project.GetRootDirectoryPath(),
                                             "output",
                                             testName);
            string configAbsPath = "/@ComponentAssemblies|kind=ComponentAssemblies|relpos=0/@AstableMultivibrator|kind=ComponentAssembly|relpos=0";


            // CyPhy2PCBMfg.CyPhy2PCBMfgInterpreter interpreter = null;

            MgaObject objTestbench = null;
            MgaFCO configObj = null;
            project.PerformInTransaction(delegate
            {
                objTestbench = project.get_ObjectByPath(pathTestbench);
                Assert.NotNull(objTestbench);

                configObj = project.get_ObjectByPath(configAbsPath) as MgaFCO;
                Assert.NotNull(configObj);
             });

            bool postToJobManager = false;
            bool keepTempModels = false;
            bool result = false;
            string outputDirectory = "";

            // Use the master Interpreter to create a JSON file in the results folder based on the Testbench's Workflow
            using (var masterInterpreter = new CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI(project))
            {
                masterInterpreter.Logger.GMEConsoleLoggingLevel = CyPhyGUIs.SmartLogger.MessageType_enum.Debug;

                var miResults = masterInterpreter.RunInTransactionOnOneConfig(objTestbench as MgaModel, configObj, postToJobManager, keepTempModels);
                outputDirectory = miResults[0].OutputDirectory;

                result = miResults.Any(x => x.Success == false) ? false : true;
            }
            Assert.True(result);
            string manifestFileName = "testbench_manifest.json";
            var pathToManifestFile = Path.Combine(outputDirectory, manifestFileName);
            Assert.True(File.Exists(pathToManifestFile));
            var fileContents = File.ReadAllText(pathToManifestFile);
            Assert.False(String.IsNullOrWhiteSpace(fileContents));

            // Use a process to start a Python script to execute the JSON file, similar to the Job Manager, but with synchronous execution.
            // See LocalPool.cs around lines 165-186, from the CyPhyMl solution, TestBenchExecutionFramework folder, JobManager project. 
            using (var proc0 = new System.Diagnostics.Process())
            {
                proc0.StartInfo.FileName = META.VersionInfo.PythonVEnvExe;
                proc0.StartInfo.Arguments = "-m testbenchexecutor testbench_manifest.json";
                proc0.StartInfo.UseShellExecute = false;
                proc0.StartInfo.CreateNoWindow = true;
                proc0.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
                proc0.StartInfo.WorkingDirectory = Path.Combine(outputDirectory);
                proc0.StartInfo.RedirectStandardError = true;
                proc0.StartInfo.RedirectStandardOutput = true;

                proc0.Start();

                StringBuilder output = new StringBuilder();
                proc0.ErrorDataReceived += (o, dataArgs) =>
                {
                    if (dataArgs.Data != null)
                    {
                        try
                        {
                            output.Append(dataArgs.Data);
                        }
                        catch (ObjectDisposedException) { }
                    }
                };
                proc0.OutputDataReceived += (o, dataArgs) =>
                {
                    if (dataArgs.Data != null)
                    {
                        try
                        {
                            output.Append(dataArgs.Data);
                        }
                        catch (ObjectDisposedException) { }
                    }
                };
                proc0.BeginOutputReadLine();
                proc0.BeginErrorReadLine();

                bool isFinished = proc0.WaitForExit(60000);   // Wait up to a minute for the workflow tests to finish.
                Assert.True(isFinished);

                proc0.Refresh();

                if (0 != proc0.ExitCode)
                {
                    Console.WriteLine("Process exit code: {0}",
                        proc0.ExitCode);
                }
                Assert.True(0 == proc0.ExitCode, output.ToString() + "  " + outputDirectory + "  " + File.ReadAllText(Path.Combine(outputDirectory, "testbench_manifest.json")));    // Check that the job finished OK.
            }

            // Check that files were created in the output path.
            string[] createdFileNames = 
            {
                "schema.boardoutline.ger",
                "schema.boardoutline.gpi",
                "schema.bottomlayer.ger",
                "schema.bottomlayer.gpi",
                "schema.bottomsilkscreen.ger",
                "schema.bottomsilkscreen.gpi",
                "schema.bottomsoldermask.ger",
                "schema.bottomsoldermask.gpi",
                "schema.drills.dri",
                "schema.drills.xln",
                "schema.tcream.ger",
                "schema.tcream.gpi",
                "schema.toplayer.ger",
                "schema.toplayer.gpi",
                "schema.topsoldermask.ger",
                "schema.topsoldermask.gpi",
                "schema_centroids.csv",
                "schema.XYRS",  // MOT-743
                "assemblyBom.csv"
            };

            foreach (string filename in createdFileNames)
            {
                var pathOutputFile = Path.Combine(outputDirectory, filename);
                Assert.True(File.Exists(pathOutputFile));
                var fileText = File.ReadAllText(pathOutputFile);
                Assert.False(String.IsNullOrWhiteSpace(fileText));
            }
        }  
    }


    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            project.CreateTerritoryWithoutSink(out mgaGateway.territory);
            mgaGateway.PerformInTransaction(del);
        }
    }

}
