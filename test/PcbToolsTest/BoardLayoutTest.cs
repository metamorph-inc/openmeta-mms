using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using META;
using Xunit;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace PcbToolsTest
{
    public class BoardLayoutTest
    {
        private String pathLayoutExecutable = Path.Combine(META.VersionInfo.MetaPath,
                                                           "bin",
                                                           "LayoutSolver.exe");

        private String pathLayouts = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length)),
                                                  "..\\..\\..\\..",
                                                  "test",
                                                  "PcbToolsTest",
                                                  "Layouts");

        private IEnumerable<String> GetLayouts()
        {
            var gitProc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = "ls-files *.json",
                    FileName = "git",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = pathLayouts,
                    CreateNoWindow = true
                }
            };

            gitProc.Start();
            gitProc.WaitForExit(10000);

            var output = gitProc.StandardOutput.ReadToEnd();

            return output.Split('\n')
                         .AsEnumerable<String>()
                         .Where(s => !String.IsNullOrWhiteSpace(s));
        }

        [Fact]
        public void Test()
        {
            var layouts = GetLayouts();
            var failures = new ConcurrentBag<String>();

            Parallel.ForEach(layouts, (pathLayout) =>
            {
                var arguments = String.Format("{0} out-{0}", Path.GetFileName(pathLayout));

                if (pathLayout.Contains("mot-638"))
                {
                    arguments += String.Format(" -i {0} -e {1} -s {2}", 0.35, 0.5, 1e7);
                }

                using (var proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = arguments,
                        CreateNoWindow = true,
                        FileName = this.pathLayoutExecutable,
                        UseShellExecute = false,
                        WorkingDirectory = pathLayouts,
                        RedirectStandardOutput = true // need to read stdout under xunit gui
                    }
                })
                {
                    proc.Start();
                    StringBuilder stdout = new StringBuilder();
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        string output = proc.StandardOutput.ReadToEnd();
                        lock (stdout)
                        {
                            stdout.Append(output);
                        }
                    }).Start();
                    if (proc.WaitForExit(120 * 1000))
                    {
                        // Completed normally
                        if (proc.ExitCode != 0)
                        {
                            lock (stdout)
                            {
                                var msg = String.Format("===== FAILURE: {0} Exit code {1} =====\n{2}",
                                                        Path.GetFileName(pathLayout), proc.ExitCode, stdout.ToString());
                                failures.Add(msg);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            proc.Kill();
                        }
                        catch (Exception)
                        {
                        }
                        // Timed out
                        var msg = String.Format("===== FAILURE: {1} ====={0}{2}",
                                                Environment.NewLine,
                                                Path.GetFileName(pathLayout),
                                                "LayoutSolver timed out");
                        failures.Add(msg);
                    }
                }
            });

            if (failures.Any())
            {
                String msg = String.Join("\n", failures);
                Assert.True(false, msg);
            }
        }
    }
}
