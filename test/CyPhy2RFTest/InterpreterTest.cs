using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Xunit;
using META;
using GME.MGA;
using GME.CSharp;
using CyPhyGUIs;

namespace CyPhy2RFTest
{
    public class InterpreterFixture : IDisposable
    {
        public static string testPath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                      "..\\..\\..\\..", "test", "CyPhy2RFTest", "model");
        private static string testXme = Path.Combine(testPath, "SimpleDipole.xme");
        private static string testAntenna = Path.Combine(testPath, "components", "1m5sgvc3", "Dipole.xml");
        private static string openEmsExecutableName = @"C:\openEMS\openEMS.exe";
        private static string nf2ffExecutable = @"C:\openEMS\nf2ff.exe";

        public MgaProject project { get; private set; }

        public InterpreterFixture()
        {
            Assert.True(File.Exists(testXme), "Test GME model '" + testXme + "' not found.");
            Assert.True(File.Exists(testAntenna), "Test antenna model '" + testAntenna + "' not found.");
            Assert.True(File.Exists(openEmsExecutableName), "Simulator executable '" + openEmsExecutableName + "' not found.");
            Assert.True(File.Exists(nf2ffExecutable), "Simulator executable '" + nf2ffExecutable + "' not found.");

            string mgaConnectionString;
            GME.MGA.MgaUtils.ImportXMEForTest(testXme, out mgaConnectionString);
            var mgaPath = mgaConnectionString.Substring("MGA=".Length);

            Assert.True(File.Exists(Path.GetFullPath(mgaPath)),
                        String.Format("{0} not found. Model import may have failed.", mgaPath));

            if (Directory.Exists(Path.Combine(testPath, "output")))
            {
                Directory.Delete(Path.Combine(testPath, "output"), true);
            }

            project = new MgaProject();
            Assert.True(project != null, "MgaProject is null");
            bool ro_mode;
            project.Open(mgaConnectionString, out ro_mode);
        }

        public void Dispose()
        {
            project.Save();
            project.Close();
            project = null;
        }
    }

    public class TestFixture
    {
        [Fact]
        public void TestInterpreterFixture()
        {
            var fixture = new InterpreterFixture();
            fixture.Dispose();
        }
    }

    public class Interpreter : IUseFixture<InterpreterFixture>
    {
        #region Fixture
        private InterpreterFixture fixture;

        public void SetFixture(InterpreterFixture fixture)
        {
            this.fixture = fixture;
        }

        private MgaProject project
        {
            get
            {
                return fixture.project;
            }
        }

        private readonly string testPath = InterpreterFixture.testPath;
        #endregion

        [Fact]
        public void CodeGenerator_CreateDirectivitySimulation_CheckGeneratedFiles()
        {
            string testbenchPath = "/@RFTest|kind=Testing|relpos=0/@DipoleTest|kind=TestBench|relpos=0";
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string pathOutput = Path.Combine(testPath, "output", testName);
            if (Directory.Exists(pathOutput))
            {
                Directory.Delete(pathOutput);
            }
            Directory.CreateDirectory(pathOutput);

            // Run the interpreter
            CyPhy2RF.CyPhy2RFInterpreter interpreter = null;
            InterpreterMainParameters parameters = null;

            project.PerformInTransaction(delegate
            {
                var objTestbench = project.get_ObjectByPath(testbenchPath);
                Assert.NotNull(objTestbench);

                var testbench = ISIS.GME.Dsml.CyPhyML.Classes.TestBench.Cast(objTestbench);
                Assert.NotNull(testbench);

                interpreter = new CyPhy2RF.CyPhy2RFInterpreter();
                interpreter.Initialize(project);

                parameters = new InterpreterMainParameters()
                {
                    CurrentFCO = testbench.Impl as MgaFCO,
                    SelectedFCOs = null,
                    Project = project,
                    OutputDirectory = pathOutput,
                    ProjectDirectory = project.GetRootDirectoryPath(),
                    config = new CyPhy2RF.CyPhy2RF_Settings()
                    {
                        doDirectivity = "true"                      
                    }
                };
            });

            interpreter.Main(parameters);

            // Check batch file (directivity)
            string batchFileName = Path.Combine(testPath, "output", testName, "run_dir_simulation.cmd");
            Assert.True(File.Exists(batchFileName), "Batch file '" + batchFileName + "' not found.");

            // Check OpenEMS input XML file
            string simulationXmlFileName = Path.Combine(testPath, "output", testName, "openEMS_input.xml");
            Assert.True(File.Exists(simulationXmlFileName), "Simulation input file '" + simulationXmlFileName + "' not found.");

            // Check NF2FF input XML file
            string nf2ffXmlFileName = Path.Combine(testPath, "output", testName, "openEMS_input.xml");
            Assert.True(File.Exists(nf2ffXmlFileName), "NF2FF input file '" + nf2ffXmlFileName + "' not found.");

            // Run FDTD postprocess
            Process p = new Process();
            int result = -1;
            try
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = batchFileName;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                result = p.ExitCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Assert.True(result == 0, "Running openEMS simulations failed.");

            // Check metrics in manifest
            string manifestPath = Path.Combine(testPath, "output", testName);
            var manifest = AVM.DDP.MetaTBManifest.OpenForUpdate(Path.Combine(manifestPath));
            Assert.True(manifest.Metrics.FirstOrDefault(m => m.Name == "Directivity") != null, String.Format("Directivity metric in manifest file not found in {0}", manifestPath));
        }
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del)
        {
            var mgaGateway = new MgaGateway(project);
            mgaGateway.PerformInTransaction(del);
        }
    }
}
