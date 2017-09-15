using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using GME.MGA;
using GME.CSharp;
using META;
using Microsoft.Win32;
using System.Reflection;

namespace CyPhy2CADPCBTest
{
    public class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length),
                //"/noshadow",
            });
            Console.In.ReadLine();
            return ret;
        }
    }

    public class TestFixture : IDisposable
    {
        public static String path_Test = Path.GetFullPath(
                                                Path.Combine(
                                                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()
                                                                                                    .CodeBase
                                                                                                    .Substring("file:///".Length)),
                                                    "..\\..\\..\\..",
                                                    "models",
                                                    "CADAssemblyTest"));

        private static String path_XME = Path.Combine(path_Test,
                                                      "CADAssemblyTest.xme");

        public readonly String path_MGA;
        public MgaProject proj { get; private set; }

        public TestFixture()
        {
            String mgaConnectionString;
            // n.b. another test uses Schematic_test.mga, so use a different name
            GME.MGA.MgaUtils.ImportXMEForTest(path_XME,
                                              Path.Combine(Path.GetDirectoryName(path_XME),
                                                           Path.GetFileNameWithoutExtension(path_XME) + "_CyPhy2CADPCB_test.mga"),
                                              out mgaConnectionString);

            path_MGA = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(path_MGA)),
                        String.Format("{0} not found. Model import may have failed.", path_MGA));

            if (Directory.Exists(Path.Combine(path_Test, "output")))
            {
                Directory.Delete(Path.Combine(path_Test, "output"), true);
            }
            if (Directory.Exists(Path.Combine(path_Test, "results")))
            {
                Directory.Delete(Path.Combine(path_Test, "results"), true);
            }

            proj = new MgaProject();
            bool ro_mode;
            proj.Open("MGA=" + Path.GetFullPath(path_MGA), out ro_mode);
            proj.EnableAutoAddOns(true);

            // Ensure "~/Documents/eagle" exists
            var pathDocEagle = Path.Combine(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"),
                                            "Documents",
                                            "eagle");
            if (!Directory.Exists(pathDocEagle))
            {
                Directory.CreateDirectory(pathDocEagle);
            }
        }

        public void Dispose()
        {
            proj.Save();
            proj.Close();
            proj = null;
        }
    }

    public class FixtureTest
    {
        [Fact]
        public void Test()
        {
            using (var fixture = new TestFixture()) 
            {
                // Intentionally left empty. 
                // Does the fixture initialize without failure?
            }
        }
    }

    public class Test : IUseFixture<TestFixture>
    {
        #region Fixture
        TestFixture fixture;
        public void SetFixture(TestFixture data)
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
                return TestFixture.path_Test;
            }
        }

        private void RunSchematicTestBench(string TestbenchPath, string OutputBasePath)
        {
            string placeRouteOutputDir = OutputBasePath + "_PlaceRoute";

            // Run PlaceRoute interpreter/analysis
            RunInterpreterMainSchematic(placeRouteOutputDir,
                                        TestbenchPath,
                                        new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" });

            string batchFileName = "placement.bat";
            var pathBatchFile = Path.Combine(placeRouteOutputDir,
                                             batchFileName);
            Assert.True(File.Exists(pathBatchFile));

            string generatedBoardFileName = "layout.json";
            var pathBoardFile = Path.Combine(placeRouteOutputDir,
                                             generatedBoardFileName);

            // Run the "placement.bat" batch file
            var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + batchFileName + "\"")
            {
                WorkingDirectory = placeRouteOutputDir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(processInfo))
            {
                bool success = process.WaitForExit(10000);
                Assert.True(success);

                // Read the streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Assert.True(0 == process.ExitCode, output + "\n\n" + error);
            }
        }

        [Fact]
        public void CheckFreeCadInstall()
        {
            string freecadInstall = QueryFreeCADInstall();
            Assert.True(!String.IsNullOrWhiteSpace(freecadInstall), "FreeCAD install was unable to be located!");
        }

        public string QueryFreeCADInstall()
        {
            // Check LocalMachine_64
            var view64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key2 = view64.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key2.GetSubKeyNames())
            {
                RegistryKey subkey2 = key2.OpenSubKey(keyName);
                string displayName2 = subkey2.GetValue("DisplayName") as string;
                if (displayName2 == null)
                {
                    continue;
                }
                if (displayName2.StartsWith("FreeCAD", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return subkey2.GetValue("InstallLocation") as string;
                }
            }

            // Check LocalMachine_32
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                string displayName = subkey.GetValue("DisplayName") as string;
                if (displayName == null)
                {
                    continue;
                }
                if (displayName.StartsWith("FreeCAD", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return subkey.GetValue("InstallLocation") as string;
                }
            }

            return "";
        }

        [Fact]
        public void RunPCBAssemblerOnSavedLayout()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;


            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/VisualizeSavedLayout|kind=TestBench|relpos=0";

            var settings = new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
            {
                useSavedLayout = "true",
                visualizerType = "STEP",
                layoutFile = "layout.json",
                runLayout = "false"
            };

            bool result = RunInterpreterMainCADPCB(OutputDir, TestbenchPath, settings);
            Assert.True(result, "Interpreter run was unsuccessful");

            RunPythonAssemblerScript(OutputDir, "step");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }

        [Fact]
        public void RunPlaceRouteAndPCBAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;


            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/PlaceRouteAndVisualize|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                   TestbenchPath,
                                                   new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                   {
                                                    useSavedLayout = "false",
                                                    visualizerType = "STEP",
                                                    layoutFile = "layout.json",
                                                    runLayout = "true"
                                                   },
                                                   true);
            Assert.True(result, "Interpreter run was unsuccessful");

            RunPythonAssemblerScript(OutputDir, "step");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }

        #region Testing of various formatted CAD files

        #region STEP Formatted Assembler Tests
        [Fact]
        void AllStepAndStepAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/AllStep|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(  OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STP",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "step");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }

        [Fact]
        void AllStlAndStepAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/AllStl|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(  OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STEP",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.False(result, "Interpreter run was unsuccessful");
        }

        [Fact]
        void MixAndStepAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/Mix|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STEP",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "step");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 5, 0, 2));
        }

        [Fact]
        void MultipleCADAndStepAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/MultipleCADPerPart|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STEP",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "step");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }
        #endregion

        #region STL Formatted Assembler Tests
        [Fact]
        void AllStepAndStlAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/AllStep|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STL",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.False(result, "Interpreter run was unsuccessful");
        }

        [Fact]
        void AllStlAndStlAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/AllStl|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STL",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "stl");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.stl")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 0));
        }
        
        [Fact]
        void MixAndStlAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/Mix|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STL",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "stl");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.stl")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 5, 0, 2));
        }

        [Fact]
        void MultipleCADAndStlAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/MultipleCADPerPart|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "STL",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "stl");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.stl")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }
        #endregion

        #region MIX Formatted Assembler Tests
        [Fact]
        void AllStepAndMixAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/AllStep|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "mix",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "mix");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }

        [Fact]
        void AllStlAndMixAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/AllStl|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "mix",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "mix");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.stl")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 0));
        }

        [Fact]
        void MixAndMixAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/Mix|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "mix",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "mix");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.stl")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 5, 0, 0));
        }

        [Fact]
        void MultipleCADAndMixAssembler()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@Format_Tests|kind=Testing|relpos=0/MultipleCADPerPart|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "mix",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "mix");

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.step")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 3, 0, 1));
        }
        #endregion

        #endregion

        [Fact]
        public void CheckModuleTemplateFound()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@UsesModuleTemplate|kind=TestBench|relpos=0";

            var settings = new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
            {
                useSavedLayout = "false",
                visualizerType = "STEP",
                runLayout = "true"
            };

            bool result = RunInterpreterMainCADPCB(OutputDir, TestbenchPath, settings);
            Assert.True(result, "Interpreter run was unsuccessful");

            Assert.True(File.Exists(Path.Combine(OutputDir, "AraTemplateParts.json")));
        }

        [Fact]
        public void CheckModuleTemplateFound_AlternateCategoryName()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/@UsesModuleTemplateAltCategory|kind=TestBench|relpos=0";

            var settings = new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
            {
                useSavedLayout = "false",
                visualizerType = "STEP",
                runLayout = "true"
            };

            bool result = RunInterpreterMainCADPCB(OutputDir, TestbenchPath, settings);
            Assert.True(result, "Interpreter run was unsuccessful");

            Assert.True(File.Exists(Path.Combine(OutputDir, "AraTemplateParts.json")));
        }

        [Fact]
        void InterferenceAnalysisSuccessTest()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/InterferenceTest|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "mix",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "mix", true);

            Assert.True(File.Exists(Path.Combine(OutputDir, "Test.stl")));
            Assert.True(CheckLogForComponentsAddedString(Path.Combine(OutputDir, "log", "CADAssembler.log"), 5, 0, 0));
            
            string log = Path.Combine(OutputDir, "log", "interference_report.log");
            Assert.True(File.Exists(log));
            Assert.True(CheckInterferenceLogForNoInterferences(log));
        }

        [Fact]
        void InterferenceAnalysisFailTest()
        {
            string TestName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string OutputDir = Path.Combine(TestPath,
                                            "output",
                                            TestName);

            string TestbenchPath = "/@TestBenches|kind=Testing|relpos=0/@Assemble_PCB_Tests|kind=Testing|relpos=0/InterferenceTest_Fail|kind=TestBench|relpos=0";

            RunSchematicTestBench(TestbenchPath, OutputDir);

            // Run the visualizer interpreter/analysis
            bool result = RunInterpreterMainCADPCB(OutputDir,
                                                     TestbenchPath,
                                                     new CyPhy2CADPCB.CyPhy2CADPCB_Settings()
                                                     {
                                                         useSavedLayout = "false",
                                                         visualizerType = "step",
                                                         layoutFile = "layout.json",
                                                         runLayout = "true"
                                                     },
                                                    true);
            Assert.True(result, "Interpreter run was unsuccessful");
            RunPythonAssemblerScript(OutputDir, "step", true);

            string log = Path.Combine(OutputDir, "log", "interference_report.log");
            Assert.True(File.Exists(log));
            Assert.False(CheckInterferenceLogForNoInterferences(log));
        }

        private bool CheckInterferenceLogForNoInterferences(string log)
        {
            // Check last line of interference log to ensure no interferences were detected.
            return File.ReadLines(log).Last().Contains("No interferences detected!");
        }

        private bool CheckLogForComponentsAddedString(string log, int topcomps, int bottomcomps, int placeholders)
        {
            // Provides better checking of tests. Check that the log files specifies the expected number of
            // components on the top and bottom of PCB, as well as number of placeholders generated.
            bool foundline = false;
            StreamReader reader = File.OpenText(log);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("Chips on Top: "))
                {
                    // Found line of interest
                    foundline = true;
                    break;
                }
            }
            if (foundline)
            {
                string[] items = line.Split(':');
                Assert.True(items[4][1].ToString() == topcomps.ToString());
                Assert.True(items[5][1].ToString() == bottomcomps.ToString());
                Assert.True(items[6][1].ToString() == placeholders.ToString());
            }

            return foundline;
        }

        private void RunPythonAssemblerScript(string OutputDir, string format, bool interference_check = false)
        {
            string interference = "";
            if (interference_check)
                interference = "-i";

            string visualizePyPath = "-m CADVisualizer Test " + format + " -a " + interference;

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = META.VersionInfo.PythonVEnvExe,
                    Arguments = visualizePyPath,
                    WorkingDirectory = OutputDir,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                procOutput = new StringBuilder("");
                procError = new StringBuilder("");

                process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
                process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorHandler);

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var timeout = 60 * 1000;    // 60 seconds

                Assert.True(process.WaitForExit(timeout), "FreeCAD Assembler __main__.py timed out");

                var msg = String.Join(Environment.NewLine, String.Format("python {0} in {1} failed:", visualizePyPath, OutputDir),
                    procOutput.ToString(), procError.ToString());
                Assert.True(0 == process.ExitCode, msg);
            }
        }

        private StringBuilder procOutput;
        private int numOutputLines = 0;
        private StringBuilder procError;
        private int numErrorLines = 0;

        private void ProcessOutputHandler(object sendingProcess,
                                          DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                numOutputLines++;

                // Add the text to the collected output.
                procOutput.Append(Environment.NewLine +
                    "[" + numOutputLines.ToString() + "] - " + outLine.Data);
            }
        }

        private void ProcessErrorHandler(object sendingProcess,
                                         DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                numErrorLines++;

                // Add the text to the collected output.
                procError.Append(Environment.NewLine +
                    "[" + numErrorLines.ToString() + "] - " + outLine.Data);
            }
        }

        private bool RunInterpreterMainCADPCB(
            string outputdirname,
            string testBenchPath,
            CyPhy2CADPCB.CyPhy2CADPCB_Settings config = null,
            bool copyLayout = false)
        {
            if (Directory.Exists(outputdirname))
            {
                Directory.Delete(outputdirname, true);
            }
            Directory.CreateDirectory(outputdirname);
            Assert.True(Directory.Exists(outputdirname), "Output directory wasn't created for some reason.");

            if (copyLayout)
            {
                File.Copy(Path.Combine(outputdirname + "_PlaceRoute", "layout.json"),
                          Path.Combine(outputdirname, "layout.json"));
            }

            MgaFCO testObj = null;
            project.PerformInTransaction(delegate
            {
                testObj = project.ObjectByPath[testBenchPath] as MgaFCO;
            });

            var interpreter = new CyPhy2CADPCB.CyPhy2CADPCBInterpreter();
            interpreter.Initialize(project);

            var mainParameters = new CyPhyGUIs.InterpreterMainParameters()
            {
                config = (config == null) ? new CyPhy2CADPCB.CyPhy2CADPCB_Settings() { Verbose = false }
                                          : config,
                Project = project,
                CurrentFCO = testObj,
                SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs")),
                StartModeParam = 128,
                ConsoleMessages = false,
                ProjectDirectory = project.GetRootDirectoryPath(),
                OutputDirectory = outputdirname
            };

            var result = interpreter.Main(mainParameters);
            interpreter.DisposeLogger();
            return result.Success;
        }

        private void RunInterpreterMainSchematic(
            string outputdirname,
            string testBenchPath,
            CyPhy2Schematic.CyPhy2Schematic_Settings config = null)
        {
            if (Directory.Exists(outputdirname))
            {
                Directory.Delete(outputdirname, true);
            }
            Directory.CreateDirectory(outputdirname);
            Assert.True(Directory.Exists(outputdirname), "Output directory wasn't created for some reason.");

            MgaFCO testObj = null;
            project.PerformInTransaction(delegate
            {
                testObj = project.ObjectByPath[testBenchPath] as MgaFCO;
            });

            var interpreter = new CyPhy2Schematic.CyPhy2SchematicInterpreter();
            interpreter.Initialize(project);

            var mainParameters = new CyPhyGUIs.InterpreterMainParameters()
            {
                config = (config == null) ? new CyPhy2Schematic.CyPhy2Schematic_Settings() { doPlaceRoute = "true" }
                                          : config,
                Project = project,
                CurrentFCO = testObj,
                SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs")),
                StartModeParam = 128,
                ConsoleMessages = false,
                ProjectDirectory = project.GetRootDirectoryPath(),
                OutputDirectory = outputdirname
            };

            var result = interpreter.Main(mainParameters);
            interpreter.DisposeLogger();

            Assert.True(result.Success, "Interpreter run was unsuccessful");
        }
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            mgaGateway.PerformInTransaction(del, abort: false);
        }
    }
}
