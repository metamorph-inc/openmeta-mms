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
            // Reset block name cache (interpreters don't necessarily get reloaded between executions, so
            // we need to reset any static variables ourselves that need to be reset between executions)
            SimulinkBlock.ResetBlockNameCache();

            var model = new SimulinkModel(selectedTestBench);

            // Copy support files
            CopySupportFile(outputDirectory, "CreateOrOverwriteModel.m", Resources.CreateOrOverwriteModel);
            CopySupportFile(outputDirectory, "PopulateTestBenchParams.py", Resources.PopulateTestBenchParams);

            CopyCopyFiles(selectedTestBench, projectDirectory, outputDirectory);

            var postProcessScripts = GetAndCopyPostProcessScripts(selectedTestBench, projectDirectory, outputDirectory);

            using (var writer = File.CreateText(Path.Combine(outputDirectory, "build_simulink.m.in")))
            {
                model.GenerateSimulinkModelCode(writer);
            }

            using (var writer = File.CreateText(Path.Combine(outputDirectory, "run_simulink.m")))
            {
                model.GenerateSimulinkExecutionCode(writer);
            }

            using (var writer = File.CreateText(Path.Combine(outputDirectory, "run.cmd")))
            {
                GenerateRunCmd(writer, postProcessScripts);
            }
        }

        private static void GenerateRunCmd(TextWriter writer, IList<string> postProcessScripts )
        {
            writer.WriteLine("\"{0}\" PopulateTestBenchParams.py", META.VersionInfo.PythonVEnvExe);
            writer.WriteLine("matlab.exe -nodisplay -nosplash -nodesktop -wait -r \"diary('matlab.out.txt'), try, run('build_simulink'), run('run_simulink'), catch me, disp('An error occurred while building or executing the model:'), fprintf('%%s / %%s\\n',me.identifier,me.message), exit(1), end, exit(0)\"");

            foreach (var script in postProcessScripts)
            {
                writer.WriteLine("\"{0}\" \"{1}\"", META.VersionInfo.PythonVEnvExe, script);
            }
        }

        private static void CopySupportFile(string outputDirectory, string fileName, string contents)
        {
            using (var writer = File.CreateText(Path.Combine(outputDirectory, fileName)))
            {
                writer.Write(contents);
            }
        }

        private static IList<string> GetAndCopyPostProcessScripts(TestBench selectedTestBench, string projectDirectory, string outputDirectory)
        {
            var scripts = new List<string>();

            foreach (var postprocessItem in selectedTestBench.Children.PostProcessingCollection)
            {
                if (postprocessItem.Attributes.ScriptPath != "")
                {
                    var fileNameOnly = Path.GetFileName(postprocessItem.Attributes.ScriptPath);

                    if (fileNameOnly != null)
                    {
                        if (File.Exists(Path.Combine(outputDirectory, fileNameOnly)))
                        {
                            GMEConsole.Warning.WriteLine(
                                    "PostProcessing script {0} already exists in output directory", fileNameOnly);
                        }
                        else
                        {
                            File.Copy(Path.Combine(projectDirectory, postprocessItem.Attributes.ScriptPath), Path.Combine(outputDirectory, fileNameOnly));
                            scripts.Add(fileNameOnly);
                        }

                    }
                }
            }

            return scripts;
        }

        private static void CopyCopyFiles(TestBench selectedTestBench, string projectDirectory, string outputDirectory)
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
                                File.Copy(Path.Combine(projectDirectory, param.Attributes.Value), Path.Combine(outputDirectory, fileNameOnly));
                            }
                        }
                    }
                    else if (param.Name == "UserLibrary" && param.Attributes.Value != "")
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
                                File.Copy(Path.Combine(projectDirectory, param.Attributes.Value), Path.Combine(outputDirectory, fileNameOnly));
                            }
                        }
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}
