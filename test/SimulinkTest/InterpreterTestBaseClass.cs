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
