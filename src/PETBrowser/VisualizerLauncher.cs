using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PETBrowser
{
    public class VisualizerLauncher
    {
        public class VisualizerExitedEventArgs : EventArgs
        {
            public string ConfigPath { get; set; }

            public VisualizerExitedEventArgs(string configPath)
            {
                ConfigPath = configPath;
            }
        }

        public static event EventHandler<VisualizerExitedEventArgs> VisualizerExited;

        private static HashSet<string> runningVisualizerSessions = new HashSet<string>();

        public static void LaunchVisualizer(string vizConfigPath)
        {
            Console.WriteLine(vizConfigPath);
            string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "OpenMETA_Visualizer_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments =
                    String.Format("/S /C \"\"{0}\" \"{1}\" \"{2}\" > \"{3}\" 2>&1\"",
                        System.IO.Path.Combine(META.VersionInfo.MetaPath, "bin\\Dig\\run.cmd"), vizConfigPath,
                        META.VersionInfo.MetaPath, logPath),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                // WorkingDirectory = ,
                // RedirectStandardError = true,
                // RedirectStandardOutput = true,
                UseShellExecute = true
                //UseShellExecute must be true to prevent R server from inheriting listening sockets from PETBrowser.exe--  which causes problems at next launch if PETBrowser terminates
            };
            var p = new Process();
            p.StartInfo = psi;
            p.EnableRaisingEvents = true;
            runningVisualizerSessions.Add(vizConfigPath);
            p.Start();

            p.Exited += (sender, args) =>
            {
                runningVisualizerSessions.Remove(vizConfigPath);
                if (VisualizerExited != null)
                {
                    VisualizerExited(null, new VisualizerExitedEventArgs(vizConfigPath));
                }
            };

            //p.Dispose();
        }

        public static bool IsVisualizerRunningForConfig(string configPath)
        {
            return runningVisualizerSessions.Contains(configPath);
        }
    }
}
