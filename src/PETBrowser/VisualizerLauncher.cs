using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Win32;

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

        private static Dictionary<string, MergedDirectoryWatcher> MergedDirectoryWatchers =
            new Dictionary<string, MergedDirectoryWatcher>();

        public static void LaunchAnalysisTool(AnalysisTool analysisTool, string vizConfigPath, Dataset mergedDataset, string dataDirectoryPath)
        {
            Console.WriteLine(vizConfigPath);

            string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Format("{0}_{1:yyyyMMdd_HHmmss}.log", analysisTool.InternalName, DateTime.Now));
            var exePath = ExpandAnalysisToolString(analysisTool.ExecutableFilePath, vizConfigPath, dataDirectoryPath);
            var arguments = ExpandAnalysisToolString(analysisTool.ProcessArguments, vizConfigPath, dataDirectoryPath);
            var workingDirectory = ExpandAnalysisToolString(analysisTool.WorkingDirectory, vizConfigPath, dataDirectoryPath);

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = string.Format("/S /C \"\"{0}\" {1} > \"{2}\" 2>&1\"", exePath, arguments, logPath),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory,
                // RedirectStandardError = true,
                // RedirectStandardOutput = true,
                UseShellExecute = true //UseShellExecute must be true to prevent subprocess from inheriting listening sockets from PETBrowser.exe--  which causes problems at next launch if PETBrowser terminates
            };

            if (analysisTool.ShowConsoleWindow)
            {
                psi.Arguments = string.Format("/S /C \"\"{0}\" {1}\"", exePath, arguments, logPath);
                psi.CreateNoWindow = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;
            }

            Console.WriteLine(psi.Arguments);
            var p = new Process();
            p.StartInfo = psi;
            p.EnableRaisingEvents = true;
            AddRunningSession(vizConfigPath, mergedDataset, dataDirectoryPath);
            p.Start();

            p.Exited += (sender, args) => { RemoveRunningSession(vizConfigPath, mergedDataset, dataDirectoryPath); };
        }

        private static string ExpandAnalysisToolString(string input, string exportPath, string workingDirectory)
        {
            string result = input.Replace("%1", exportPath);
            result = result.Replace("%2", workingDirectory);
            //TODO: %3 should be list of all selected directories; can we get that?
            result = result.Replace("%4", META.VersionInfo.MetaPath);

            return result;
        }

        private static void RemoveRunningSession(string vizConfigPath, Dataset mergedDataset, string dataDirectoryPath)
        {
            var mergedFolderPath = Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory,
                mergedDataset.Folders[0]);
            runningVisualizerSessions.Remove(vizConfigPath);

            var watcher = MergedDirectoryWatchers[mergedFolderPath];
            watcher.DecrementReferenceCount();

            if (watcher.ReferenceCount == 0)
            {
                watcher.Dispose();
                MergedDirectoryWatchers.Remove(mergedFolderPath);
            }

            if (VisualizerExited != null)
            {
                VisualizerExited(null, new VisualizerExitedEventArgs(vizConfigPath));
            }
        }

        private static void AddRunningSession(string vizConfigPath, Dataset mergedDataset, string dataDirectoryPath)
        {
            var mergedFolderPath = Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory,
                mergedDataset.Folders[0]);
            runningVisualizerSessions.Add(vizConfigPath);

            if (MergedDirectoryWatchers.ContainsKey(mergedFolderPath))
            {
                MergedDirectoryWatchers[mergedFolderPath].IncrementReferenceCount();
            }
            else
            {
                MergedDirectoryWatchers[mergedFolderPath] = new MergedDirectoryWatcher(mergedDataset, dataDirectoryPath);
            }
        }

        public static bool IsVisualizerRunningForConfig(string configPath)
        {
            return runningVisualizerSessions.Contains(configPath);
        }

        private class MergedDirectoryWatcher : IDisposable
        {
            public int ReferenceCount { get; private set; }

            private List<FileSystemWatcher> Watchers { get; set; }

            private Dataset MergedDataset { get; set; }
            private string DataDirectoryPath { get; set; }

            public MergedDirectoryWatcher(Dataset mergedDataset, string dataDirectoryPath)
            {
                ReferenceCount = 1;
                Watchers = new List<FileSystemWatcher>();
                MergedDataset = mergedDataset;
                DataDirectoryPath = dataDirectoryPath;

                var mergedDirectory = Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory,
                    mergedDataset.Folders[0]);
                List<Dataset> sourceDatasets = new List<Dataset>();
                
                using (
                    var reader =
                        File.OpenText(Path.Combine(mergedDirectory, "metadata.json")))
                {
                    var serializer = new JsonSerializer();
                    var metadata = (MergedPetMetadata)serializer.Deserialize(reader, typeof(MergedPetMetadata));

                    sourceDatasets.AddRange(metadata.SourceDatasets);
                }

                foreach (var dataset in sourceDatasets)
                {
                    if (dataset.Kind == Dataset.DatasetKind.PetResult)
                    {
                        foreach (var folder in dataset.Folders)
                        {
                            var testbenchManifestFilePath = Path.Combine(dataDirectoryPath, DatasetStore.ResultsDirectory, folder);
                            var directoryToWatch = Directory.GetParent(testbenchManifestFilePath).FullName;
                            Console.WriteLine("Watching {0}", directoryToWatch);
                            var watcher = new FileSystemWatcher(directoryToWatch);
                            //watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                            watcher.Filter = "output.csv";

                            watcher.Changed += OnChanged;
                            watcher.Created += OnChanged;
                            watcher.Deleted += OnChanged;
                            watcher.Renamed += OnRenamed;

                            watcher.EnableRaisingEvents = true;

                            Watchers.Add(watcher);
                        }
                    }
                    else
                    {
                        // Don't live refresh anything except PetResult datasets
                    }
                }

                

                Console.WriteLine("Started watching dataset at {0}", mergedDirectory);
            }

            public void Dispose()
            {
                foreach (var watcher in Watchers)
                {
                    watcher.Dispose();
                }
            }

            public void IncrementReferenceCount()
            {
                ReferenceCount++;
            }

            public void DecrementReferenceCount()
            {
                ReferenceCount--;
            }

            private void OnChanged(object source, FileSystemEventArgs e)
            {
                // Specify what is done when a file is changed, created, or deleted.
                Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
                PetMerger.RefreshMergedPet(MergedDataset, DataDirectoryPath);
            }

            private void OnRenamed(object source, RenamedEventArgs e)
            {
                // Specify what is done when a file is renamed.
                Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
                PetMerger.RefreshMergedPet(MergedDataset, DataDirectoryPath);
            }
        }
    }
}
