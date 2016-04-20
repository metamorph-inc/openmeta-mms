using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace PcbToolsTest
{
    public class Test
    {
        [Fact]
        public void PreRoutedNominal()
        {
            //////// Layout Solver section
            var pathLayoutJson = Path.Combine(pathTest, "layout.json");
            if (File.Exists(pathLayoutJson))
            {
                File.Delete(pathLayoutJson);
            }

            using (var proc = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo()
                    {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = pathLayoutSolver,
                        Arguments = "layout-input.json layout.json",
                        WorkingDirectory = pathTest,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                })
            {
                proc.Start();
                Assert.True(proc.WaitForExit(5000));
                Assert.True(0 == proc.ExitCode, "Non-zero exit code " + proc.ExitCode.ToString() + ": " + proc.StandardError.ReadToEnd());
            }
            
            // Check that output file is different than input.
            // It's not a great test, but it's a start.
            Assert.True(File.Exists(pathLayoutJson));
            var jsonLayoutJson = File.ReadAllText(pathLayoutJson);
            var jsonLayoutInputJson = File.ReadAllText(Path.Combine(pathTest, "layout-input.json"));
            Assert.NotEqual(jsonLayoutInputJson, jsonLayoutJson);


            //////// Board Synthesis section
            var pathSchemaSch = Path.Combine(pathTest, "schema.sch");
            var orgContentsSchemaSch = File.ReadAllText(pathSchemaSch);

            var pathSchemaBrd = Path.Combine(pathTest, "schema.brd");
            if (File.Exists(pathSchemaBrd))
            {
                File.Delete(pathSchemaBrd);
            }

            using (var proc = new System.Diagnostics.Process()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo()
                    {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = pathBoardSynthesis,
                        Arguments = "schema.sch layout.json",
                        WorkingDirectory = pathTest,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                })
            {
                proc.Start();
                Assert.True(proc.WaitForExit(5000));
                Assert.Equal(0, proc.ExitCode);
                Assert.True(0 == proc.ExitCode, proc.StandardError.ToString());
            }

            // Ensure schema.sch hasn't changed
            Assert.Equal(orgContentsSchemaSch, File.ReadAllText(pathSchemaSch));

            // Ensure board file was generated
            Assert.True(File.Exists(pathSchemaBrd));
        }

        private static String pathLayoutSolver = Path.Combine(META.VersionInfo.MetaPath,
                                                              "bin",
                                                              "LayoutSolver.exe");

        private static String pathBoardSynthesis
        {
            get
            {
                return BoundingBoxCalc.pathBoardSynthesis;
            }
        }

        private static String pathTest = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                                                      "..\\..\\..\\..",
                                                      "test",
                                                      "PcbToolsTest",
                                                      "PreRouted");
    }
}
