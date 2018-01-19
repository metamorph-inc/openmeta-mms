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
            /*
%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"

SET QUERY_ERRORLEVEL=%ERRORLEVEL%

IF %QUERY_ERRORLEVEL% neq 0 (
    echo on
    echo "META tools not installed." >> _FAILED.txt
    echo "See Error Log: _FAILED.txt"
    exit %QUERY_ERRORLEVEL%
)

FOR /F "skip=2 tokens=2,*" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\META" /v "META_PATH"') DO SET META_PATH=%%B
SET META_PYTHON_EXE="%META_PATH%\bin\Python27\Scripts\Python.exe"
REM Interpreter-specific stuff goes here
IF %ERRORLEVEL% neq 0 (
    echo on
    echo "Simulink simulation failed." >> _FAILED.txt
    exit %ERRORLEVEL%
)
            */
            writer.WriteLine("%SystemRoot%\\SysWoW64\\REG.exe query \"HKLM\\software\\META\" /v \"META_PATH\"\r\n\r\nSET QUERY_ERRORLEVEL=%ERRORLEVEL%\r\n\r\nIF %QUERY_ERRORLEVEL% neq 0 (\r\n    echo on\r\n    echo \"META tools not installed.\" >> _FAILED.txt\r\n    echo \"See Error Log: _FAILED.txt\"\r\n    exit %QUERY_ERRORLEVEL%\r\n)\r\n\r\nFOR /F \"skip=2 tokens=2,*\" %%A IN (\'%SystemRoot%\\SysWoW64\\REG.exe query \"HKLM\\software\\META\" /v \"META_PATH\"\') DO SET META_PATH=%%B\r\nSET META_PYTHON_EXE=\"%META_PATH%\\bin\\Python27\\Scripts\\Python.exe\"");
            writer.WriteLine("%META_PYTHON_EXE% PopulateTestBenchParams.py");
            writer.WriteLine("matlab.exe -nodisplay -nosplash -nodesktop -wait -r \"diary('matlab.out.txt'), try, run('build_simulink'), run('run_simulink'), catch me, disp('An error occurred while building or executing the model:'), fprintf('%%s / %%s\\n',me.identifier,me.message), exit(1), end, exit(0)\"");

            foreach (var script in postProcessScripts)
            {
                writer.WriteLine("%META_PYTHON_EXE% \"{0}\"", script);
            }
            writer.WriteLine("IF %ERRORLEVEL% neq 0 (\r\n    echo on\r\n    echo \"Simulink simulation failed.\" >> _FAILED.txt\r\n    exit %ERRORLEVEL%\r\n)");
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
