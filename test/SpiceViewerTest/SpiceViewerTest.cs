using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xunit;
using System.Reflection;

namespace SpiceViewerTest
{
    public class SpiceReaderFixture
    {
        private static String pathTestModel = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                           "..", "..", "..", "..",
                                                           "test",
                                                           "SpiceViewerTest",
                                                           "model");

        private static String pathSPICE = Path.Combine(META.VersionInfo.MetaPath,
                                                       "bin",
                                                       "spice",
                                                       "bin",
                                                       "ngspice.exe");

        public static String pathRAWFile = Path.Combine(pathTestModel,
                                                         "schema.raw");

        public SpiceReaderFixture()
        {
            if (File.Exists(pathRAWFile))
            {
                return;
            }

            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = pathSPICE,
                    Arguments = "-b -r schema.raw -o schema.log schema.cir",
                    WorkingDirectory = pathTestModel,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            int minsToWait = 8;
            if (process.WaitForExit(1000 * 60 * minsToWait) == false)
            {
                process.Kill();
                throw new TimeoutException(String.Format("{0} did not complete in {1} minutes",
                                                         process.StartInfo.FileName,
                                                         minsToWait));
            }
            Assert.Equal(0, process.ExitCode);
        }
    }

    public class SpiceReader : IUseFixture<SpiceReaderFixture>
    {
        #region fixture
        private SpiceReaderFixture fixture;
        public void SetFixture(SpiceReaderFixture data)
        {
            fixture = data;
        }
        #endregion

        [Fact]
        private void TestSpiceViewer()
        {
            String pathSpiceRead = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                "..", "..", "..", "..",
                                                "src",
                                                "Python27Packages",
                                                "spice_viewer",
                                                "spice_read.py");

            // Test parsing of RAW file
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    Arguments = String.Format("-m SpiceVisualizer.spicedatareader \"{0}\"", SpiceReaderFixture.pathRAWFile),
                    WorkingDirectory = Path.GetDirectoryName(pathSpiceRead),
                    FileName = META.VersionInfo.PythonVEnvExe,
                    //Arguments = String.Join(" ", pathSpiceRead, SpiceReaderFixture.pathRAWFile),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            StringBuilder outputData = new StringBuilder();
            process.OutputDataReceived += (o, dataArgs) =>
            {
                if (dataArgs.Data != null)
                {
                    try
                    {
                        outputData.Append(dataArgs.Data);
                    }
                    catch (ObjectDisposedException) { }
                }
            };

            StringBuilder errorData = new StringBuilder();
            process.ErrorDataReceived += (o, dataArgs) =>
            {
                if (dataArgs.Data != null)
                {
                    try
                    {
                        errorData.Append(dataArgs.Data);
                    }
                    catch (ObjectDisposedException) { }
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            int minsToWait = 15;
            if (process.WaitForExit(1000 * 60 * minsToWait) == false)
            {
                process.Kill();
                throw new TimeoutException(String.Format("{0} did not complete in {1} minutes",
                                                         process.StartInfo.FileName,
                                                         minsToWait));
            }

            Console.Out.Write(outputData.ToString());
            Console.Error.Write(errorData.ToString());

            Assert.Equal(0, process.ExitCode);
            Assert.True(outputData.ToString().Contains("Title:  rc time delay circuit"));
        }
    }

    class Program
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

}
