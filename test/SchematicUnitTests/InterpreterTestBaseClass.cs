using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using GME.MGA;
using META;
using System.Diagnostics;

namespace SchematicUnitTests
{
    public abstract class InterpreterTestBaseClass
    {
        public abstract MgaProject project { get; }

        public abstract String TestPath { get; }

        public void RunInterpreterMain(string outputdirname, string testBenchPath, CyPhy2Schematic.CyPhy2Schematic_Settings config = null)
        {
            var result = RunInterpreterMainAndReturnResult(outputdirname, testBenchPath, config);

            Assert.True(result.Success, "Interpreter run was unsuccessful");
        }

        public CyPhyGUIs.IInterpreterResult RunInterpreterMainAndReturnResult(string outputdirname, string testBenchPath, CyPhy2Schematic.CyPhy2Schematic_Settings config = null)
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

                var interpreter = new CyPhy2Schematic.CyPhy2SchematicInterpreter();
                interpreter.Initialize(project);

                var mainParameters = new CyPhyGUIs.InterpreterMainParameters()
                {
                    config = (config == null) ? new CyPhy2Schematic.CyPhy2Schematic_Settings() { Verbose = false }
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
                interpreter.DisposeLogger();
            }, abort: true);

            return result;
        }

        public static String RunPlaceOnly(string OutputDir)
        {
            string batchFileName = "placeonly.bat";
            var pathBatchFile = Path.Combine(OutputDir,
                                             batchFileName);
            Assert.True(File.Exists(pathBatchFile));

            string generatedBoardFileName = "schema.brd";
            var pathBoardFile = Path.Combine(OutputDir,
                                             generatedBoardFileName);

            // Run the "placeonly.bat" batch file
            var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + batchFileName + "\"")
            {
                WorkingDirectory = OutputDir,
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

                Assert.True(0 == process.ExitCode, output + Environment.NewLine + error);
            }

            // Check that the "schema.brd" file exists.
            Assert.True(File.Exists(pathBoardFile), "Failed to generate " + generatedBoardFileName);
            return pathBoardFile;
        }
    }
}
