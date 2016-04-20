using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using Xunit;
using System.Threading;

namespace PcbToolsTest
{
    public class LayoutFailureTest
    {
        private String pathLayoutExecutable = Path.Combine(META.VersionInfo.MetaPath,
                                                           "bin",
                                                           "LayoutSolver.exe");

        private String pathLayouts = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                  "..\\..\\..\\..",
                                                  "test",
                                                  "PcbToolsTest",
                                                  "BadLayouts");
        
        private String RunLayout(String InputLayoutFileName, String AdditionalArgs = null)
        {
            string LayoutFileName = String.Format("copy-{0}", InputLayoutFileName);
            File.Copy(Path.Combine(pathLayouts, InputLayoutFileName), Path.Combine(pathLayouts, LayoutFileName), overwrite: true);
            var arguments = String.Format("{0} out-{0}", LayoutFileName);
            if (!String.IsNullOrWhiteSpace(AdditionalArgs))
            {
                arguments += " " + AdditionalArgs;
            }
            
            StringBuilder output = new StringBuilder();

            using (var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = arguments,
                    CreateNoWindow = true,
                    FileName = this.pathLayoutExecutable,
                    UseShellExecute = false,
                    WorkingDirectory = pathLayouts,
                    RedirectStandardOutput = true, // need to read stdout under xunit gui
                    RedirectStandardError = true
                }
            })
            {
                proc.Start();
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    string stdout = proc.StandardOutput.ReadToEnd();

                    lock (output)
                    {
                        output.Append(stdout);
                    }
                }).Start();
                string stderr = proc.StandardError.ReadToEnd();
                lock (output)
                {
                    output.Append(stderr);
                }

                if (proc.WaitForExit(120 * 1000))
                {
                    // Completed normally
                    // Don't care about exit code for this test
                }
                else
                {
                    // Timed out

                    try
                    {
                        proc.Kill();
                    }
                    catch (Exception)
                    {
                    }
                    
                    var msg = String.Format("===== FAILURE: {1} ====={0}{2}",
                                            Environment.NewLine,
                                            LayoutFileName,
                                            "LayoutSolver timed out");
                    Assert.False(true, msg);
                }
            }

            return output.ToString();
        }
        
        [Fact]
        public void EdgeViolate()
        {
            var layoutFileName = "edge-violate.json";
            var args = "-e 0.2";
            var output = RunLayout(layoutFileName, args);

            Assert.Contains("ERROR:", output);
            Assert.Contains("Unsolvable exact-X constraint on package", output);
            Assert.Contains("component is partially off the left side of the board.", output);
        }
        
        [Fact]
        public void ExactOverlap()
        {
            var layoutFileName = "exact-overlap.json";
            var args = "--incremental";
            var output = RunLayout(layoutFileName, args);

            Assert.Contains("Search failed with no solution", output);
            Assert.Contains("causes conflict", output);
        }
    }
}
