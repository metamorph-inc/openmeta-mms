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

        public DatasetStore(string dataDirectory)
        {
            this.DataDirectory = dataDirectory;

            ResultDatasets = new List<Dataset>();
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

                    if (!datasets.ContainsKey(time))
                    {
                        datasets[time] = new Dataset(Dataset.DatasetKind.Result, time, name);
                    }

                    var thisDataset = datasets[time];
                    thisDataset.Count++;
                    thisDataset.Folders.Add(folder);
                }
                else
                {
                    // Don't add non-PET runs to the browser
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

        public void ArchiveSelectedDatasets(string archiveName)
        {
            var archivePath = Path.Combine(this.DataDirectory, ArchiveDirectory, archiveName + ".csv");

            WriteSelectedDatasetsToCsv(archivePath, false);
        }

        public string ExportSelectedDatasetsToViz()
        {
            var exportPath = Path.Combine(this.DataDirectory, ResultsDirectory, "mergedPET.csv");

            WriteSelectedDatasetsToCsv(exportPath, true);

            return exportPath;
        }

        private void WriteSelectedDatasetsToCsv(string csvPath, bool writeNoneAsEmpty)
        {
            List<string> headers = null;

            using (var outputCsvFile = File.CreateText(csvPath))
            {
                var writer = new CsvWriter(outputCsvFile);
                foreach (var d in ResultDatasets)
                {
                    WriteDatasetToCsv(d, ref headers, writer, writeNoneAsEmpty);
                }

                foreach (var d in ArchiveDatasets)
                {
                    WriteDatasetToCsv(d, ref headers, writer, writeNoneAsEmpty);
                }
            }
        }

        private void WriteDatasetToCsv(Dataset d, ref List<string> headers, CsvWriter writer, bool writeNoneAsEmpty)
        {
            if (d.Selected)
            {
                Console.WriteLine(d.Name);

                foreach (var folder in d.Folders)
                {
                    bool firstHeaderReadForFolder = true;

                    string csvFileName;
                    if (d.Kind == Dataset.DatasetKind.Result)
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
    }

    public class Dataset
    {
        public enum DatasetKind
        {
            Result,
            Archive
        }

        public DatasetKind Kind { get; set; } 
        public string Time { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public List<string> Folders { get; private set; }
        public bool Selected { get; set; }

        public Dataset(DatasetKind kind, string time, string name)
        {
            this.Kind = kind;
            this.Time = time;
            this.Name = name;
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
