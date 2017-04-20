using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using GME.MGA;
using META;
using System.Diagnostics;
using GME.CSharp;

namespace SchematicUnitTests
{
    public abstract class InterpreterTestBaseClass
    {
        public abstract MgaProject project { get; }

        public abstract String TestPath { get; }

        public void RunInterpreterMain(string outputdirname, string testBenchPath, CyPhy2Simulink.CyPhy2Simulink_Settings config = null)
        {
            var result = RunInterpreterMainAndReturnResult(outputdirname, testBenchPath, config);

            Assert.True(result.Success, "Interpreter run was unsuccessful");
        }

        public CyPhyGUIs.IInterpreterResult RunInterpreterMainAndReturnResult(string outputdirname, string testBenchPath, CyPhy2Simulink.CyPhy2Simulink_Settings config = null)
        {
            if (Directory.Exists(outputdirname))
            {
                foreach (string filename in Directory.GetFiles(outputdirname, "*", SearchOption.AllDirectories))
                {
                    File.Delete(Path.Combine(outputdirname, filename));
                }

            }
            Directory.CreateDirectory(outputdirname);
            Assert.True(Directory.Exists(outputdirname), "Output directory wasn't created for some reason.");

            CyPhyGUIs.IInterpreterResult result = null;
            project.PerformInTransaction(delegate
            {
                MgaFCO testObj = null;
                testObj = project.ObjectByPath[testBenchPath] as MgaFCO;
                Assert.NotNull(testObj);

                var interpreter = new CyPhy2Simulink.CyPhy2SimulinkInterpreter();
                interpreter.Initialize(project);

                var mainParameters = new CyPhyGUIs.InterpreterMainParameters()
                {
                    config = (config == null) ? new CyPhy2Simulink.CyPhy2Simulink_Settings() { Verbose = false }
                                              : config,
                    Project = project,
                    CurrentFCO = testObj,
                    SelectedFCOs = (MgaFCOs)Activator.CreateInstance(Type.GetTypeFromProgID("Mga.MgaFCOs")),
                    StartModeParam = 128,
                    ConsoleMessages = false,
                    ProjectDirectory = project.GetRootDirectoryPath(),
                    OutputDirectory = outputdirname
                };

                result = interpreter.Main(mainParameters);
                //interpreter.DisposeLogger();
            }, abort: true);

            return result;
        }

        /**
         * Runs simulink generator's run.cmd; returns the process exit code
         */
        public int RunSimulinkGen(string outputDir)
        {
            string batchFileName = "run.cmd";
            var pathBatchFile = Path.Combine(outputDir,
                batchFileName);
            Assert.True(File.Exists(pathBatchFile));

            // Run the "placeonly.bat" batch file
            var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + batchFileName + "\"")
            {
                WorkingDirectory = outputDir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit(10000);

                // Read the streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                return process.ExitCode;
            }
        }

        #region Helper functions (common asserts used by multiple tests)

        protected void AssertTestBenchSucceeds(string testName, string testbenchPath, out string outputDir)
        {
            outputDir = Path.Combine(TestPath,
                "output",
                testName);

            var result = RunInterpreterMainAndReturnResult(outputDir, testbenchPath);

            Assert.True(result.Success, "Interpreter should succeed");

            AssertCommonSimulinkFilesGenerated(outputDir);
        }

        protected void AssertTestBenchSucceeds(string testName, string testbenchPath)
        {
            string dummy;
            AssertTestBenchSucceeds(testName, testbenchPath, out dummy);
        }

        protected void AssertTestBenchFails(string testName, string testbenchPath)
        {
            string outputDir = Path.Combine(TestPath,
                "output",
                testName);

            var result = RunInterpreterMainAndReturnResult(outputDir, testbenchPath);

            Assert.False(result.Success, "Interpreter should fail");
        }

        /*
         * A number of files should always be generated if the interpreter succeeds--
         * verify that they're present
         */
        protected void AssertCommonSimulinkFilesGenerated(string outputDir)
        {
            AssertFileExists(outputDir, "build_simulink.m.in");
            AssertFileExists(outputDir, "run_simulink.m");
            AssertFileExists(outputDir, "CreateOrOverwriteModel.m");
            AssertFileExists(outputDir, "PopulateTestBenchParams.py");
            AssertFileExists(outputDir, "run.cmd");
        }

        protected static void AssertFileExists(string outputDir, string fileName)
        {
            Assert.True(File.Exists(Path.Combine(outputDir, fileName)), string.Format("{0} should exist", fileName));
        }

        #endregion
    }

    internal static class Utils
    {
        public static void PerformInTransaction(this MgaProject project, MgaGateway.voidDelegate del, bool abort)
        {
            var mgaGateway = new MgaGateway(project);
            mgaGateway.PerformInTransaction(del, abort: abort);
        }
    }
}
