using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CyPhy2Simulink.Properties;
using GME.CSharp;
using ISIS.GME.Dsml.CyPhyML.Interfaces;
using File = System.IO.File;

namespace CyPhy2Simulink.Simulink
{
    class SimulinkGenerator
    {
        public static GMEConsole GMEConsole { get; set; }

        public static void GenerateSimulink(TestBench selectedTestBench, string outputDirectory, string projectDirectory)
        {
            var model = new SimulinkModel(selectedTestBench);

            // Copy support files
            CopySupportFile(outputDirectory, "CreateOrOverwriteModel.m", Resources.CreateOrOverwriteModel);
            CopySupportFile(outputDirectory, "PopulateTestBenchParams.py", Resources.PopulateTestBenchParams);

            CopyCopyFiles(selectedTestBench, outputDirectory);

            using (var writer = File.CreateText(Path.Combine(outputDirectory, "build_simulink.m")))
            {
                model.GenerateSimulinkModelCode(writer);
            }

            using (var writer = File.CreateText(Path.Combine(outputDirectory, "run_simulink.m")))
            {
                model.GenerateSimulinkExecutionCode(writer);
            }

            using (var writer = File.CreateText(Path.Combine(outputDirectory, "run.cmd")))
            {
                GenerateRunCmd(writer);
            }
        }

        private static void GenerateRunCmd(TextWriter writer)
        {
            writer.WriteLine("matlab.exe -nodisplay -nosplash -nodesktop -wait -r \"diary('matlab.out.txt'), try, run('build_simulink.m'), run('run_simulink.m'), catch me, fprintf('%s / %s\\n',me.identifier,me.message), exit(1), end, exit(0)\"");
        }

        private static void CopySupportFile(string outputDirectory, string fileName, string contents)
        {
            using (var writer = File.CreateText(Path.Combine(outputDirectory, fileName)))
            {
                writer.Write(contents);
            }
        }

        private static void CopyCopyFiles(TestBench selectedTestBench, string outputDirectory)
        {
            foreach (var param in selectedTestBench.Children.ParameterCollection)
            {
                if (!param.AllDstConnections.Any() && !param.AllSrcConnections.Any())
                {
                    if (param.Name == "CopyFile" && param.Attributes.Value != "")
                    {
                        var fileNameOnly = Path.GetFileName(param.Attributes.Value);
                        if (fileNameOnly != null)
                        {
                            if (File.Exists(Path.Combine(outputDirectory, fileNameOnly)))
                            {
                                GMEConsole.Warning.WriteLine(
                                    "Attempted to copy file {0} which already exists in output directory", fileNameOnly);
                            }
                            else
                            {
                                File.Copy(param.Attributes.Value, Path.Combine(outputDirectory, fileNameOnly));
                            }
                        }
                    }
                    else if (param.Name == "UserLibrary")
                    {
                        //TODO: support external user libraries
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}
