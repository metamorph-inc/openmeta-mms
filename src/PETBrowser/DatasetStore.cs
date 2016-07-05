using System;
using System.Collections.Generic;
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

namespace PETBrowser
{
    public class DatasetStore
    {
        public const string ResultsDirectory = "results";
        public const string ArchiveDirectory = "archive";
        private const string MetadataFilename = "results.metaresults.json";

        public string DataDirectory { get; private set; }

        public ResultsMetadata Metadata { get; private set; }

        public List<Dataset> ResultDatasets { get; private set; }
        public List<Dataset> ArchiveDatasets { get; private set; }
        public List<Dataset> TestbenchDatasets { get; private set; }

        public DatasetStore(string dataDirectory)
        {
            this.DataDirectory = dataDirectory;

            ResultDatasets = new List<Dataset>();
            TestbenchDatasets = new List<Dataset>();
            LoadResultDatasets();

            ArchiveDatasets = new List<Dataset>();
            LoadArchiveDatasets();
        }

        private void LoadResultDatasets()
        {
            Dictionary<string, Dataset> datasets = new Dictionary<string, Dataset>();

            var metadataPath = Path.Combine(DataDirectory, ResultsDirectory, MetadataFilename);
            using (var metadataFile = File.OpenText(metadataPath))
            {
                var serializer = new JsonSerializer();
                this.Metadata = (ResultsMetadata) serializer.Deserialize(metadataFile, typeof(ResultsMetadata));
            }

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
                            var mdaoJson = (JObject) JToken.ReadFrom(jsonReader);

                            names.AddRange(((JObject) mdaoJson["components"]).Properties().Select(property => property.Name));
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
            }

            ResultDatasets.Clear();
            ResultDatasets.AddRange(datasets.Values);
        }

        public void LoadArchiveDatasets()
        {
            ArchiveDatasets.Clear();

            var archiveDirectory = Path.Combine(DataDirectory, ArchiveDirectory);

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
        public void DeleteDataset(Dataset datasetToDelete, bool deleteAllFiles)
        {
            if (datasetToDelete.Kind == Dataset.DatasetKind.Archive)
            {
                //Archives aren't represented in metadata; just delete the archive file
                var archivePath = Path.Combine(this.DataDirectory, ArchiveDirectory, datasetToDelete.Folders[0]);
                File.Delete(archivePath);
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
                    serializer.Serialize(metadataFile, Metadata);
                }

                // Delete all associated files and folders if deleteAllFiles is true
                if (deleteAllFiles)
                {
                    foreach (var path in datasetToDelete.Folders)
                    {
                        var directoryToDelete =
                            Directory.GetParent(Path.Combine(this.DataDirectory, ResultsDirectory, path));

                        directoryToDelete.Delete(true);
                    }
                }
            }
        }

        public void ArchiveSelectedDatasets(string archiveName, Dataset highlightedDataset)
        {
            var archivePath = Path.Combine(this.DataDirectory, ArchiveDirectory, archiveName + ".csv");

            WriteSelectedDatasetsToCsv(archivePath, false, highlightedDataset);
        }

        public string ExportSelectedDatasetsToViz(Dataset highlightedDataset, bool highlightedDatasetOnly = false)
        {
            var exportPath = Path.Combine(this.DataDirectory, ResultsDirectory, "mergedPET.csv");

            WriteSelectedDatasetsToCsv(exportPath, true, highlightedDataset, highlightedDatasetOnly);

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
                    }
                    else
                    {
                        throw new ArgumentException("No data exported; no datasets were selected to export");
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

                string csvFileName;
                if (d.Kind == Dataset.DatasetKind.PetResult)
                {
                    csvFileName = Path.Combine(this.DataDirectory, ResultsDirectory,
                        folder.Replace("testbench_manifest.json", "output.csv"));
                }
                else
                {
                    csvFileName = Path.Combine(this.DataDirectory, ArchiveDirectory, folder);
                }

                using (var csvFile = File.OpenText(csvFileName))
                {
                    var csvReader = new CsvReader(csvFile, new CsvConfiguration()
                    {
                        HasHeaderRecord = true
                    });

                    while (csvReader.Read())
                    {
                        if (headers == null)
                        {
                            Console.Out.WriteLine(csvFileName);
                            headers = new List<string>(csvReader.FieldHeaders);
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
                            if (firstHeaderReadForFolder)
                            {
                                var otherHeaders = new List<string>(csvReader.FieldHeaders);
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
                        writer.NextRecord();
                    }
                }
            }
        }
    }

    public class Dataset
    {
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
        public bool Selected { get; set; }

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
