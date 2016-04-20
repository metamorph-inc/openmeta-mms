using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TonkaACMTest
{
    class Common
    {
        public static int RunCyPhyMLComparator(string desired, string imported)
        {
            var path = Path.GetDirectoryName(desired);
            var comparatorExe = Path.Combine(META.VersionInfo.MetaPath, "src", "bin", "CyPhyMLComparator.exe");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = comparatorExe
                }
            };

            process.StartInfo.Arguments += desired;
            process.StartInfo.Arguments += " " + imported;

            return processCommon(process, true);
        }

        public static int processCommon(Process process, bool redirect = false)
        {
            using (process)
            {
                process.StartInfo.UseShellExecute = false;

                if (redirect)
                {
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                }

                process.StartInfo.CreateNoWindow = true;
                process.Start();
                if (redirect)
                {
                    char[] buffer = new char[4096];
                    while (true)
                    {
                        int read = process.StandardError.Read(buffer, 0, 4096);
                        if (read == 0)
                        {
                            break;
                        }
                        Console.Error.Write(buffer, 0, read);
                    }

                    buffer = new char[4096];
                    while (true)
                    {
                        int read = process.StandardOutput.Read(buffer, 0, 4096);
                        if (read == 0)
                        {
                            break;
                        }
                        Console.Out.Write(buffer, 0, read);
                    }
                }
                process.WaitForExit();

                return process.ExitCode;
            }
        }

        public static int RunXmlComparator(string exported, string desired)
        {
            string xmlComparatorPath = Path.Combine(
                Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                "..\\..\\..\\..",
                "test",
                "InterchangeTest",
                "InterchangeXmlComparator",
                "bin",
                "Release",
                "InterchangeXmlComparator.exe"
                );

            var process = new Process
            {
                StartInfo =
                {
                    FileName = xmlComparatorPath
                }
            };

            process.StartInfo.Arguments += String.Format(" -e {0} -d {1} -m Component", exported, desired);
            return processCommon(process, true);
        }
    }
}
