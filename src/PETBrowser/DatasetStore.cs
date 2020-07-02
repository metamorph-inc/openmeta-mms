using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using META;
using AVM.DDP;
using System.Text.RegularExpressions;

namespace PETBrowser
{
    public class DatasetStore
    {
        public const string ResultsDirectory = "results";
        public const string ArchiveDirectory = "archive";
        public const string MergedDirectory = "merged";
        public const string DeletedDirectory = "_deleted";
        private const string MetadataFilename = "results.metaresults.json";
        private const int ProgressUpdateInterval = 100;

        public string DataDirectory { get; private set; }

        public ResultsMetadata Metadata { get; private set; }

        public List<Dataset> ResultDatasets { get; private set; }
        public List<Dataset> ArchiveDatasets { get; private set; }
        public List<Dataset> MergedDatasets { get; private set; }
        public List<Dataset> TestbenchDatasets { get; private set; }

        public HashSet<string> TrackedResultsFolders { get; private set; }

        public delegate void ProgressCallback(int completed, int total);

        public DatasetStore(string dataDirectory, ProgressCallback progressCallback)
        {
            this.DataDirectory = dataDirectory;

            TrackedResultsFolders = new HashSet<string>();

            ResultDatasets = new List<Dataset>();
            
            TestbenchDatasets = new List<Dataset>();
            LoadResultDatasets(progressCallback);

            ArchiveDatasets = new List<Dataset>();
            LoadArchiveDatasets();

            MergedDatasets = new List<Dataset>();
            LoadMergedDatasets();
        }

        private void LoadResultDatasets(ProgressCallback progressCallback)
        {
            Dictionary<string, Dataset> datasets = new Dictionary<string, Dataset>();

            var metadataPath = Path.Combine(DataDirectory, ResultsDirectory, MetadataFilename);
            using (var metadataFile = File.OpenText(metadataPath))
            {
                var serializer = new JsonSerializer();
                this.Metadata = (ResultsMetadata)serializer.Deserialize(metadataFile, typeof(ResultsMetadata));
            }

            var totalResultsCount = this.Metadata.Results.Count;
            var processedResultsCount = 0;

            foreach (var result in this.Metadata.Results)
            {
                var time = result.Time;
                var folder = result.Summary;
                var mdaoName = Path.Combine(DataDirectory, ResultsDirectory,
                    folder.Replace("testbench_manifest", "mdao_config"));

                if (File.Exists(mdaoName))
                {
                    if (!datasets.ContainsKey(time))
                    {
                        using (var mdaoFile = File.OpenText(mdaoName))
                        {
                            var serializer = new JsonSerializer();
                            var mdaoConfig = (PETConfig) serializer.Deserialize(mdaoFile, typeof(PETConfig));

                            var name = "";

                            if (!string.IsNullOrEmpty(mdaoConfig.PETName))
                            {
                                var splitName = mdaoConfig.PETName.Split('/');
                                name = splitName.Last();
                            }
                            else
                            {
                                //Fall back to list of components when we don't have the PET name available in
                                //mdao_config.json (older PET runs from before we added it to the config file)
                                var names = new List<string>();

                                names.AddRange(mdaoConfig.components.Keys);

                                var nameBuilder = new StringBuilder();
                                nameBuilder.Append("[");
                                nameBuilder.Append(string.Join(",", names));
                                nameBuilder.Append("]");
                                name = nameBuilder.ToString();
                            }

                            datasets[time] = new Dataset(Dataset.DatasetKind.PetResult, time, name);
                        }
                        
                    }

                    var thisDataset = datasets[time];
                    thisDataset.Count++;
                    thisDataset.Folders.Add(folder);
                    var actualDirectory = Directory.GetParent(Path.Combine(DataDirectory, ResultsDirectory, folder));
                    TrackedResultsFolders.Add(actualDirectory.FullName);
                }
                else
                {
                    // Non-PET run; add it as a TestBench
                    var testbenchManifestFilePath = Path.Combine(DataDirectory, ResultsDirectory, folder);

                    try
                    {
                        using (var testbenchManifestFile = File.OpenText(testbenchManifestFilePath))
                        using (var jsonReader = new JsonTextReader(testbenchManifestFile))
                        {
                            var manifestJson = (JObject) JToken.ReadFrom(jsonReader);

                            var testBenchName = (string) manifestJson["TestBench"];

                            var newDataset = new Dataset(Dataset.DatasetKind.TestBenchResult, time, testBenchName);
                            newDataset.Status = (string) manifestJson["Status"];
                            newDataset.DesignName = (string) manifestJson["DesignName"];

                            newDataset.Count++;
                            newDataset.Folders.Add(folder);

                            var actualDirectory =
                                Directory.GetParent(Path.Combine(DataDirectory, ResultsDirectory, folder));
                            TrackedResultsFolders.Add(actualDirectory.FullName);

                            TestbenchDatasets.Add(newDataset);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //Don't add testbench if we don't find a corresponding testbenchmanifest in its results folder
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //Don't add testbench if we don't find its directory
                    }
                    catch (JsonException)
                    {
                        //Don't add testbench if the TestbenchManifest.json is unparseable (like if a job were cancelled and its corresponding manifest were corrupted)
                    }
                }

                processedResultsCount++;

                if (processedResultsCount % ProgressUpdateInterval == 0)
                {
                    progressCallback(processedResultsCount, totalResultsCount);
                }
            }

            ResultDatasets.Clear();
            ResultDatasets.AddRange(datasets.Values);
        }

        public void LoadArchiveDatasets()
        {
            ArchiveDatasets.Clear();

            var archiveDirectory = Path.Combine(DataDirectory, ArchiveDirectory);

            if (!Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            foreach (var file in Directory.EnumerateFiles(archiveDirectory))
            {
                var fileBasename = Path.GetFileName(file);
                if (fileBasename != null && file.EndsWith(".csv"))
                {
                    var newDataset = new Dataset(Dataset.DatasetKind.Archive, File.GetCreationTime(file).ToString("yyyy-MM-dd HH-mm-ss"), fileBasename);
                    newDataset.Count++;
                    newDataset.Folders.Add(fileBasename);

                    ArchiveDatasets.Add(newDataset);
                }
            }
        }

        public void LoadMergedDatasets()
        {
            MergedDatasets.Clear();

            var mergedDirectory = Path.Combine(DataDirectory, MergedDirectory);

            if (!Directory.Exists(mergedDirectory))
            {
                Directory.CreateDirectory(mergedDirectory);
            }

            foreach (var directory in Directory.EnumerateDirectories(mergedDirectory))
            {
                //Consider a merged directory to be valid if it contains a metadata.json (the others may or may not exist)
                var mergedPetFile = Path.Combine(directory, "metadata.json");
                if (File.Exists(mergedPetFile))
                {
                    using (var reader = File.OpenText(mergedPetFile))
                    {
                        var serializer = new JsonSerializer();
                        var metadata = (MergedPetMetadata)serializer.Deserialize(reader, typeof(MergedPetMetadata));

                        var directoryName = Path.GetFileName(directory);
                        Dataset newDataset;
                        if (metadata.Kind == MergedPetMetadata.MergedPetKind.MergedPet)
                        {
                            newDataset = new Dataset(Dataset.DatasetKind.MergedPet,
                                File.GetCreationTime(mergedPetFile).ToString("yyyy-MM-dd HH-mm-ss"), directoryName);
                        }
                        else
                        {
                            newDataset = new Dataset(Dataset.DatasetKind.Pet,
                                File.GetCreationTime(mergedPetFile).ToString("yyyy-MM-dd HH-mm-ss"), directoryName);
                        }
                        newDataset.Count = metadata.SourceDatasets.Aggregate(0, (total, nextDataset) => total + nextDataset.Folders.Count);
                        newDataset.Folders.Add(directoryName);
                        
                        metadata.SourceDatasets.ForEach(dataset => newDataset.SourceFolders.AddRange(dataset.Folders.Select(
                            folder =>
                            {
                                string path;
                                switch (dataset.Kind)
                                {
                                    case Dataset.DatasetKind.PetResult:
                                        path = Path.Combine(DataDirectory, ResultsDirectory,
                                            folder.Replace("testbench_manifest.json", "")); ;
                                        break;
                                    case Dataset.DatasetKind.Archive:
                                        path = Path.Combine(DataDirectory, ArchiveDirectory, folder);
                                        break;
                                    case Dataset.DatasetKind.TestBenchResult:
                                        throw new ArgumentException("Merged PETs should not include test benches");
                                    case Dataset.DatasetKind.MergedPet:
                                    case Dataset.DatasetKind.Pet:
                                        path = Path.Combine(DataDirectory, MergedDirectory, folder);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                return Path.GetFullPath(path);
                            })));

                        MergedDatasets.Add(newDataset);
                    }
                }
            }
        }

        /**
         * Deletes a dataset from results.metaresults.json (or from the Archive folder as appropriate).
         * The caller is responsible for reloading the list of datasets if desired.
         */
        public void DeleteDataset(Dataset datasetToDelete)
        {
            if (datasetToDelete.Kind == Dataset.DatasetKind.Archive)
            {
                //Archives aren't represented in metadata; just delete the archive file
                var archivePath = Path.Combine(this.DataDirectory, ArchiveDirectory, datasetToDelete.Folders[0]);

                var deletedDirectory = Directory.CreateDirectory(Path.Combine(DataDirectory, DeletedDirectory));

                var deletedBaseName = Path.GetFileName(archivePath);
                var deletedBasePath = deletedDirectory.FullName;
                var deletedCandidateName = deletedBaseName;

                var i = 0;

                while (File.Exists(Path.Combine(deletedBasePath, deletedCandidateName)))
                {
                    i++;
                    deletedCandidateName = string.Format("{0} ({1})", deletedBaseName, i);
                }

                File.Move(archivePath, Path.Combine(deletedBasePath, deletedCandidateName));
            }
            else if (datasetToDelete.Kind == Dataset.DatasetKind.MergedPet || datasetToDelete.Kind == Dataset.DatasetKind.Pet)
            {
                var mergedPath = Path.Combine(this.DataDirectory, MergedDirectory, datasetToDelete.Folders[0]);

                var deletedDirectory = Directory.CreateDirectory(Path.Combine(DataDirectory, DeletedDirectory));

                var deletedBaseName = Path.GetFileName(mergedPath);
                var deletedBasePath = deletedDirectory.FullName;
                var deletedCandidateName = deletedBaseName;

                var i = 0;

                while (Directory.Exists(Path.Combine(deletedBasePath, deletedCandidateName)))
                {
                    i++;
                    deletedCandidateName = string.Format("{0} ({1})", deletedBaseName, i);
                }

                Directory.Move(mergedPath, Path.Combine(deletedBasePath, deletedCandidateName));
            }
            else
            {
                // Remove all results where folder ("Summary") is in the dataset's Folders list
                Metadata.Results.RemoveAll(item => datasetToDelete.Folders.Contains(item.Summary));

                // Now update results.metaresults.json
                var metadataPath = Path.Combine(DataDirectory, ResultsDirectory, MetadataFilename);
                using (var metadataFile = File.CreateText(metadataPath))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(metadataFile, Metadata);
                }

                foreach (var path in datasetToDelete.Folders)
                {
                    var directoryToDelete =
                        Directory.GetParent(Path.Combine(this.DataDirectory, ResultsDirectory, path));

                    MoveFolderToDeleted(directoryToDelete);
                }
            }
        }

        public void Cleanup(ProgressCallback callback)
        {
            var resultsDirectory = new DirectoryInfo(Path.Combine(DataDirectory, ResultsDirectory));

            var directories = resultsDirectory.EnumerateDirectories();
            var totalCount = directories.Count();
            var deletedCount = 0;

            foreach (var subdirectory in resultsDirectory.EnumerateDirectories())
            {
                if (!TrackedResultsFolders.Contains(subdirectory.FullName))
                {
                    MoveFolderToDeleted(subdirectory);
                    deletedCount++;

                    if (deletedCount % ProgressUpdateInterval == 0)
                    {
                        callback(deletedCount, totalCount);
                    }
                }
            }
        }

        public void MoveFolderToDeleted(DirectoryInfo directoryToMove)
        {
            var deletedDirectory = Directory.CreateDirectory(Path.Combine(DataDirectory, DeletedDirectory));

            var deletedBaseName = Path.GetFileName(directoryToMove.Name);
            var deletedBasePath = deletedDirectory.FullName;
            var deletedCandidateName = deletedBaseName;

            var i = 0;

            while (Directory.Exists(Path.Combine(deletedBasePath, deletedCandidateName)))
            {
                i++;
                deletedCandidateName = string.Format("{0} ({1})", deletedBaseName, i);
            }

            directoryToMove.MoveTo(Path.Combine(deletedBasePath, deletedCandidateName));
        }
    }

    public class Dataset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum DatasetKind
        {
            PetResult = 0,
            Archive = 1,
            TestBenchResult = 2,
            MergedPet = 3,
            Pet = 4
        }

        public DatasetKind Kind { get; set; } 
        public string Time { get; set; }
        public string Name { get; set; }
        public string DesignName { get; set; }
        public string Status { get; set; }

        public int Count { get; set; }
        public List<string> Folders { get; private set; }
        [JsonIgnore] // Generated at runtime for datasets within a merged PET; shouldn't be serialized
        public List<string> SourceFolders { get; private set; }

        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set { PropertyChanged.ChangeAndNotify(ref _selected, value, () => Selected); }
        }

        public Dataset(DatasetKind kind, string time, string name)
        {
            this.Kind = kind;
            this.Time = time;
            this.Name = name;
            this.DesignName = "";
            this.Status = "";
            this.Count = 0;
            this.Folders = new List<string>();
            this.SourceFolders = new List<string>();
            this.Selected = false;
        }
    }

    public class ResultsMetadata
    {
        public List<ResultsMetadataItem> Results { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Results: {");

            this.Results.ForEach(result => builder.AppendFormat("  {0}\n", result));

            return builder.ToString();
        }


    }

    public class ResultsMetadataItem
    {
        public string Design { get; set; }
        // ReSharper disable once InconsistentNaming
        public string DesignID { get; set; }
        public string TestBench { get; set; }
        public string Time { get; set; }
        public string Summary { get; set; }

        public override string ToString()
        {
            return string.Format("Design: {0}, DesignID: {1}, TestBench: {2}, Time: {3}, Summary: {4}", Design, DesignID, TestBench, Time, Summary);
        }
    }
}
