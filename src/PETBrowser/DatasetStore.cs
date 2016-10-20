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

namespace PETBrowser
{
    public class DatasetStore
    {
        public const string ResultsDirectory = "results";
        public const string ArchiveDirectory = "archive";
        public const string DeletedDirectory = "_deleted";
        private const string MetadataFilename = "results.metaresults.json";
        private const int ProgressUpdateInterval = 100;

        public string DataDirectory { get; private set; }

        public ResultsMetadata Metadata { get; private set; }

        public List<Dataset> ResultDatasets { get; private set; }
        public List<Dataset> ArchiveDatasets { get; private set; }
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
                        var names = new List<string>();

                        using (var mdaoFile = File.OpenText(mdaoName))
                        using (var jsonReader = new JsonTextReader(mdaoFile))
                        {
                            var mdaoJson = (JObject)JToken.ReadFrom(jsonReader);

                            names.AddRange(((JObject)mdaoJson["components"]).Properties().Select(property => property.Name));
                        }
                        var nameBuilder = new StringBuilder();
                        nameBuilder.Append("[");
                        nameBuilder.Append(string.Join(",", names));
                        nameBuilder.Append("]");
                        var name = nameBuilder.ToString();

                        datasets[time] = new Dataset(Dataset.DatasetKind.PetResult, time, name);
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
                            var manifestJson = (JObject)JToken.ReadFrom(jsonReader);

                            var testBenchName = (string)manifestJson["TestBench"];

                            var newDataset = new Dataset(Dataset.DatasetKind.TestBenchResult, time, testBenchName);
                            newDataset.Status = (string)manifestJson["Status"];
                            newDataset.DesignName = (string)manifestJson["DesignName"];

                            newDataset.Count++;
                            newDataset.Folders.Add(folder);

                            var actualDirectory = Directory.GetParent(Path.Combine(DataDirectory, ResultsDirectory, folder));
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
                File.Move(archivePath, Path.Combine(deletedDirectory.FullName, Path.GetFileName(archivePath)));
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

            directoryToMove.MoveTo(Path.Combine(deletedDirectory.FullName, directoryToMove.Name));
        }

        public void ArchiveSelectedDatasets(string archiveName, Dataset highlightedDataset)
        {
            var archivePath = Path.Combine(this.DataDirectory, ArchiveDirectory, archiveName + ".csv");

            WriteSelectedDatasetsToCsv(archivePath, false, highlightedDataset);
        }

        public string ExportSelectedDatasetsToViz(Dataset highlightedDataset, bool highlightedDatasetOnly = false)
        {
            var exportPath = Path.Combine(this.DataDirectory, ResultsDirectory, "mergedPET.csv");
            var mappingPath = Path.Combine(this.DataDirectory, ResultsDirectory, "mappingPET.csv");

            WriteSelectedDatasetsToCsv(exportPath, true, highlightedDataset, highlightedDatasetOnly);
            WriteSelectedMappingToCsv(mappingPath, highlightedDataset, highlightedDatasetOnly);

            return exportPath;
        }

        private void WriteSelectedDatasetsToCsv(string csvPath, bool writeNoneAsEmpty, Dataset highlightedDataset, bool highlightedDatasetOnly = false)
        {
            List<string> headers = null;

            using (var outputCsvFile = File.CreateText(csvPath))
            {
                var writer = new CsvWriter(outputCsvFile);
                if (!highlightedDatasetOnly)
                {
                    foreach (var d in ResultDatasets)
                    {
                        if (d.Selected)
                        {
                            WriteDatasetToCsv(d, ref headers, writer, writeNoneAsEmpty);
                        }
                    }

                    foreach (var d in ArchiveDatasets)
                    {
                        if (d.Selected)
                        {
                            WriteDatasetToCsv(d, ref headers, writer, writeNoneAsEmpty);
                        }
                    }
                }

                if (headers == null)
                {
                    //No selected datasets; if we have a highlighted dataset, write the highlighted dataset instead
                    if (highlightedDataset != null)
                    {
                        Console.WriteLine("No selected datasets; writing highlighted dataset");
                        WriteDatasetToCsv(highlightedDataset, ref headers, writer, writeNoneAsEmpty);

                        if (headers == null)
                        {
                            throw new ArgumentException("No data exported; no valid datasets were selected to export");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("No data exported; no valid datasets were selected to export");
                    }
                }
            }
        }

        private void WriteDatasetToCsv(Dataset d, ref List<string> headers, CsvWriter writer, bool writeNoneAsEmpty)
        {
            Console.WriteLine(d.Name);

            foreach (var folder in d.Folders)
            {
                bool firstHeaderReadForFolder = true;

                var addedHeaders = new Dictionary<string, string>();
                var headersPresent = new Dictionary<string, bool>();
                addedHeaders["CfgID"] = "Unknown";

                string testbenchManifestFilePath;
                string csvFileName;
                if (d.Kind == Dataset.DatasetKind.PetResult)
                {
                    testbenchManifestFilePath = Path.Combine(DataDirectory, ResultsDirectory, folder);
                    csvFileName = Path.Combine(this.DataDirectory, ResultsDirectory,
                        folder.Replace("testbench_manifest.json", "output.csv"));

                    // Try to get the CfgID and alternatives to append to 'mergedPET.csv'
                    try
                    {
                        var Manifest = MetaTBManifest.Deserialize(testbenchManifestFilePath);

                        if (Manifest.CfgID != null)
                            addedHeaders["CfgID"] = Manifest.CfgID;

                        if (Manifest.Design != null)
                        {
                            var Decisions = FlattenDesignType(Manifest.Design).Where(a => a.Type == "Alternative" | a.Type == "Optional");
                            foreach (var Decision in Decisions)
                            {
                                var Choices = Decision.Children.Where(a => a.Selected == true);
                                if (Choices == null)
                                {
                                    addedHeaders[Decision.Name] = "None";
                                }
                                foreach (var Choice in Choices)
                                {
                                    Console.WriteLine(Choice.Name);
                                    addedHeaders[Decision.Name] = Choice.Name;
                                }
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //Don't adjust configuration name if we don't find a corresponding testbenchmanifest in its results folder
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //Don't adjust configuration name if we don't find its directory
                    }
                }
                else
                {
                    csvFileName = Path.Combine(this.DataDirectory, ArchiveDirectory, folder);
                }

                Console.WriteLine("Exporting CSV {0}", csvFileName);

                //Assume the added columns don't exist in the data we're reading from 
                foreach (var header in addedHeaders)
                    headersPresent[header.Key] = false;

                using (var csvFile = File.OpenText(csvFileName))
                {
                    try
                    {
                        var csvReader = new CsvReader(csvFile, new CsvConfiguration()
                        {
                            HasHeaderRecord = true
                        });

                        while (csvReader.Read())
                        {
                            // Is this the first dataset being written to the CSV?
                            if (headers == null)
                            {
                                // Yes. Let's setup the authority on what variables should be present.
                                Console.Out.WriteLine(csvFileName);
                                headers = new List<string>(csvReader.FieldHeaders);
                                foreach (var addedHeader in addedHeaders)
                                {
                                    if (headers.Contains(addedHeader.Key))
                                    {
                                        headersPresent[addedHeader.Key] = true;
                                    }
                                    else
                                    {
                                        headers.Add(addedHeader.Key);
                                    }
                                }
                                headers.Sort();
                                firstHeaderReadForFolder = false;
                                foreach (var header in headers)
                                {
                                    writer.WriteField<string>(header);
                                }
                                writer.NextRecord();
                            }
                            else
                            {
                                // No. Let's see if the headers match up.
                                if (firstHeaderReadForFolder)
                                {
                                    var otherHeaders = new List<string>(csvReader.FieldHeaders);
                                    foreach (var addedHeader in addedHeaders)
                                    {
                                        if (otherHeaders.Contains(addedHeader.Key))
                                        {
                                            headersPresent[addedHeader.Key] = true;
                                        }
                                        else
                                        {
                                            otherHeaders.Add(addedHeader.Key);
                                        }
                                    }
                                    otherHeaders.Sort();
                                    if (!headers.SequenceEqual(otherHeaders))
                                    {
                                        Console.WriteLine("Headers for {0} didn't match initial headers",
                                            csvFileName);
                                        break;
                                    }
                                }
                            }

                            foreach (var header in headers)
                            {
                                if (addedHeaders.ContainsKey(header) && !headersPresent[header])
                                {
                                    writer.WriteField<string>(addedHeaders[header]);
                                }
                                else
                                {
                                    string fieldValue = csvReader.GetField<string>(header);

                                    if (writeNoneAsEmpty && fieldValue == "None")
                                    {
                                        writer.WriteField<string>("");
                                    }
                                    else
                                    {
                                        writer.WriteField<string>(fieldValue);
                                    }
                                }
                            }
                            writer.NextRecord();
                        }
                    }
                    catch (CsvReaderException e)
                    {
                        Console.WriteLine("Invalid CSV found at {0}", csvFileName);
                        Trace.TraceWarning("Invalid CSV found at {0}", csvFileName);
                    }
                }
            }
        }

        private IEnumerable<MetaTBManifest.DesignType> FlattenDesignType(MetaTBManifest.DesignType e)
        {
            if(e.Type != "Component")
            {
                return (new[] { e }).Concat(e.Children.SelectMany(c => FlattenDesignType(c)));
            }
            return (new[] { e });
        } 

        private void WriteSelectedMappingToCsv(string csvPath, Dataset highlightedDataset, bool highlightedDatasetOnly = false)
        {
            Console.WriteLine("Searching for Mapping");

            using (var outputCsvFile = File.CreateText(csvPath))
            {
                var writer = new CsvWriter(outputCsvFile);

                // Try writing the mapping for one of the selected datasets
                if (!highlightedDatasetOnly)
                {
                    foreach (var d in ResultDatasets)
                    {
                        if (d.Selected)
                        {
                            if (WriteMappingToCsv(d, writer))
                                return;
                        }
                    }
                }

                // No selected datasets; if we have a highlighted dataset, write the mapping for the highlighted dataset instead
                if (highlightedDataset != null)
                {
                    Console.WriteLine("No selected datasets; writing mapping for highlighted dataset");
                    if (WriteMappingToCsv(highlightedDataset, writer))
                    {
                        return;
                    }
                }
                else
                {
                    // 'mappingPET.csv' will be empty
                }
            }
        }

        private bool WriteMappingToCsv(Dataset d, CsvWriter writer)
        {
            Console.WriteLine(d.Name);

            List<string> names = new List<string>();
            List<string> rangeMins = new List<string>();
            List<string> rangeMaxs = new List<string>();

            foreach (var folder in d.Folders)
            {
                string mdaoName;
                if (d.Kind == Dataset.DatasetKind.PetResult)
                {
                    mdaoName = Path.Combine(this.DataDirectory, ResultsDirectory,
                        folder.Replace("testbench_manifest", "mdao_config"));

                    
                    // Try to get the mapping to export to 'mappingPET.csv'
                    try
                    {
                        
                        // Parse the mdaoConfig file to get the mapping
                        using (var mdaoFile = File.OpenText(mdaoName))
                        using (var jsonReader = new JsonTextReader(mdaoFile))
                        {
                            //Console.WriteLine("Attempting to open {0}", mdaoName);
                            //Console.WriteLine("Using jsonReader: {0}", jsonReader);
                            
                            var mdaoJson = (JObject)JToken.ReadFrom(jsonReader);

                            foreach (var singleDriver in ((JObject)mdaoJson["drivers"]))
                            {
                                //Console.WriteLine(singleDriver.ToString());
                                foreach (var variable in (JObject)(((JObject)singleDriver.Value)["designVariables"]))
                                {
                                    //Console.WriteLine(variable.ToString());
                                    
                                    names.Add(variable.Key);
                                    rangeMins.Add((string)((JObject)variable.Value)["RangeMin"]);
                                    rangeMaxs.Add((string)((JObject)variable.Value)["RangeMax"]);
                                }
                            }

                            //Console.WriteLine("Names: {0}", names.Count);
                            //Console.WriteLine("rangeMins: {0}", rangeMins.Count);
                            //Console.WriteLine("rangeMaxs: {0}", rangeMaxs.Count);

                            var lists = new List<List<string>>();
                            lists.Add(names);
                            lists.Add(rangeMins);
                            lists.Add(rangeMaxs);

                            foreach (var row in lists)
                            {
                                foreach (var item in row)
                                {
                                    if (item != null)
                                        writer.WriteField<string>(item);
                                    else
                                        writer.WriteField<string>("N/A");
                                }
                                writer.NextRecord();
                            }
                            
                            return true;
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //Don't grab mapping if we don't find a corresponding mdao config file in its results folder
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //Don't grab mapping if we don't find a corresponding mdao config file in its results folder
                    }
                }
            }
            return false;
        }
    }

    public class Dataset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public enum DatasetKind
        {
            PetResult,
            Archive,
            TestBenchResult
        }

        public DatasetKind Kind { get; set; } 
        public string Time { get; set; }
        public string Name { get; set; }
        public string DesignName { get; set; }
        public string Status { get; set; }

        public int Count { get; set; }
        public List<string> Folders { get; private set; }

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
