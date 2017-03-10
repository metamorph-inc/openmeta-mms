using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AVM.DDP;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PETBrowser
{
    public class MergedPetMetadata
    {
        public List<string> PetDirectories { get; set; }

        public MergedPetMetadata()
        {
            PetDirectories = new List<string>();
        }
    }

    public class VisualizerConfig
    {
        [JsonProperty(PropertyName = "pet_config")]
        public string PetConfig { get; set; }

        [JsonProperty(PropertyName = "raw_data")]
        public string RawData { get; set; }

        [JsonProperty(PropertyName = "tabs")]
        public List<string> Tabs { get; set; }

        public VisualizerConfig()
        {
            Tabs = new List<string>();
        }
    }

    public class PetMerger
    {
        public const string MergedDirectory = "merged";

        public static void MergePets(string mergedName, IEnumerable<Dataset> datasetsToMerge, string dataDirectoryPath)
        {
            var datasets = datasetsToMerge.ToList();

            var mergedDirectory = Path.Combine(dataDirectoryPath, MergedDirectory);

            if(!Directory.Exists(mergedDirectory))
            {
                Directory.CreateDirectory(mergedDirectory);
            }

            var mergedPetDirectory = Path.Combine(mergedDirectory, mergedName);
            if (Directory.Exists(mergedPetDirectory))
            {
                throw new InvalidOperationException("A merged PET with this name already exists."); // TODO: use a better exception here
            }

            var tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectoryPath);

            try
            {
                var metadataPath = Path.Combine(tempDirectoryPath, "metadata.json");
                var exportPath = Path.Combine(tempDirectoryPath, "mergedPET.csv");
                var mappingPath = Path.Combine(tempDirectoryPath, "mappingPET.csv");
                var mergedPetConfigPath = Path.Combine(tempDirectoryPath, "pet_config.json");
                var vizConfigPath = Path.Combine(tempDirectoryPath, "visualizer_config.json");

                WriteSelectedDatasetsToCsv(exportPath, true, datasets, dataDirectoryPath, true, true, true);
                WriteSelectedMappingToCsv(mappingPath, datasets, dataDirectoryPath);
                WriteSummarizedPetConfig(mergedPetConfigPath, datasets, dataDirectoryPath);
                WriteMergedMetadata(metadataPath, datasets);
                WriteDefaultVizConfig(vizConfigPath);

                DirectoryCopy(tempDirectoryPath, mergedPetDirectory, true);
            }
            finally
            {
                // Make sure our temp directory is removed, whether we complete successfully or an error occurs
                Directory.Delete(tempDirectoryPath, true);
            }
        }

        private static void WriteMergedMetadata(string filePath, IEnumerable<Dataset> datasets)
        {
            var metadata = new MergedPetMetadata();

            foreach (var dataset in datasets.Where(dataset => dataset.Kind == Dataset.DatasetKind.PetResult))
            {
                foreach (var folder in dataset.Folders)
                {
                    var executionDirectoryPath = Path.GetDirectoryName(folder);
                    if (executionDirectoryPath != null)
                    {
                        var relativePath = Path.Combine("..", "..", DatasetStore.ResultsDirectory,
                            executionDirectoryPath);

                        metadata.PetDirectories.Add(relativePath);
                    }
                }
            }

            using (var writer = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, metadata);
            }
        }

        private static void WriteDefaultVizConfig(string vizConfigPath)
        {
            var config = new VisualizerConfig();
            config.PetConfig = "pet_config.json";
            config.RawData = "mergedPET.csv";
            config.Tabs = new List<string> { "Explore.R", "DataTable.R", "PETRefinement.R" };

            using (var writer = File.CreateText(vizConfigPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, config);
            }
        }

        private static void WriteSelectedDatasetsToCsv(string csvPath, bool writeNoneAsEmpty, IEnumerable<Dataset> datasets, string dataDirectoryPath, bool documentCfdID = false, bool documentAlternatives = false, bool documentOptionals = false)
        {
            List<string> headers = null;

            using (var outputCsvFile = File.CreateText(csvPath))
            {
                var writer = new CsvWriter(outputCsvFile);

                foreach (var d in datasets)
                {
                    if (d.Selected)
                    {
                        WriteDatasetToCsv(d, ref headers, writer, dataDirectoryPath, writeNoneAsEmpty, documentCfdID, documentAlternatives, documentOptionals);
                    }
                }

                if (headers == null)
                {
                    throw new ArgumentException("No data exported; no valid datasets were selected to export");
                }
            }
        }

        private static void WriteDatasetToCsv(Dataset d, ref List<string> headers, CsvWriter writer, string dataDirectoryPath, bool writeNoneAsEmpty, bool documentCfdID, bool documentAlternatives, bool documentOptionals)
        {
            Console.WriteLine(d.Name);

            foreach (var folder in d.Folders)
            {
                bool firstHeaderReadForFolder = true;

                var addedHeaders = new Dictionary<string, string>();
                var headersPresent = new Dictionary<string, bool>();

                if (documentCfdID)
                {
                    addedHeaders["CfgID"] = "Unknown";
                }

                string testbenchManifestFilePath;
                string csvFileName;
                if (d.Kind == Dataset.DatasetKind.PetResult)
                {
                    testbenchManifestFilePath = Path.Combine(dataDirectoryPath, DatasetStore.ResultsDirectory, folder);
                    csvFileName = Path.Combine(dataDirectoryPath, DatasetStore.ResultsDirectory,
                        folder.Replace("testbench_manifest.json", "output.csv"));

                    // Try to get the CfgID and alternatives to append to 'mergedPET.csv'
                    try
                    {
                        var Manifest = MetaTBManifest.Deserialize(testbenchManifestFilePath);

                        if (documentCfdID & Manifest.CfgID != null)
                        {
                            addedHeaders["CfgID"] = Manifest.CfgID;
                        }

                        if (Manifest.Design != null)
                        {
                            var Decisions = FlattenDesignType(Manifest.Design).Where(a => a.Type == "Alternative" | a.Type == "Optional");
                            if (!documentAlternatives)
                            {
                                Decisions = Decisions.Where(a => a.Type != "Alternative");
                            }
                            if (!documentOptionals)
                            {
                                Decisions = Decisions.Where(a => a.Type != "Optional");
                            }
                            foreach (var Decision in Decisions)
                            {
                                var Choices = Decision.Children.Where(a => DesignTypeIsSelected(a) == true);
                                if (Choices.Count() == 0)
                                {
                                    addedHeaders[Decision.Name] = "None";
                                }
                                else
                                {
                                    foreach (var Choice in Choices)
                                    {
                                        addedHeaders[Decision.Name] = Choice.Name;
                                    }
                                }
                                //Console.WriteLine("DesignType: {0} ({1}): {2}", Decision.Name, Decision.Type, addedHeaders[Decision.Name]);
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
                    csvFileName = Path.Combine(dataDirectoryPath, DatasetStore.ArchiveDirectory, folder);
                }

                Console.WriteLine("Exporting CSV {0}", csvFileName);

                //Assume the added columns don't exist in the data we're reading from 
                foreach (var header in addedHeaders)
                    headersPresent[header.Key] = false;

                using (var csvFile = new StreamReader(File.Open(csvFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
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

        private static void WriteSummarizedPetConfig(string mergedPetConfigPath, IEnumerable<Dataset> datasets, string dataDirectoryPath)
        {
            //Compute the list of mdao_configs to summarize
            List<string> petConfigPaths = new List<string>();

            foreach (var d in datasets.Where(dataset => dataset.Kind == Dataset.DatasetKind.PetResult))
            {
                foreach (var folder in d.Folders)
                {
                    petConfigPaths.Add(Path.Combine(dataDirectoryPath, DatasetStore.ResultsDirectory,
                        folder.Replace("testbench_manifest.json", "mdao_config.json")));
                }
            }

            PETConfig mergedConfig = null;

            foreach (var path in petConfigPaths)
            {
                try
                {
                    using (var configReader = File.OpenText(path))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        var petConfig = (PETConfig)serializer.Deserialize(configReader, typeof(PETConfig));

                        if (mergedConfig == null)
                        {
                            //Assume all mdao_configs are the same; we just use the first one as our merged config
                            mergedConfig = petConfig;
                        }
                        else
                        {
                            var selectedConfigurations = petConfig.SelectedConfigurations;
                            if (selectedConfigurations != null)
                            {
                                if (mergedConfig.SelectedConfigurations == null)
                                {
                                    mergedConfig.SelectedConfigurations = new List<string>();
                                }
                                mergedConfig.SelectedConfigurations.AddRange(selectedConfigurations);
                            }
                        }
                    }
                }
                catch (JsonException e)
                {
                    Console.WriteLine("Invalid JSON found at {0}", path);
                    Trace.TraceWarning("Invalid JSON found at {0}", path);
                }
            }

            if (mergedConfig != null)
            {
                using (var writer = File.CreateText(mergedPetConfigPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, mergedConfig);
                }
            }
        }

        private static IEnumerable<MetaTBManifest.DesignType> FlattenDesignType(MetaTBManifest.DesignType e)
        {
            if (e.Type != "Component")
            {
                return (new[] { e }).Concat(e.Children.SelectMany(c => FlattenDesignType(c)));
            }
            return (new[] { e });
        }

        private static bool DesignTypeIsSelected(MetaTBManifest.DesignType e)
        {
            if (e.Selected == true)
            {
                return true;
            }
            if (e.Children != null)
            {
                foreach (var child in e.Children)
                {
                    if (DesignTypeIsSelected(child))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void WriteSelectedMappingToCsv(string csvPath, IEnumerable<Dataset> datasets, string dataDirectoryPath)
        {
            Console.WriteLine("Searching for Mapping");

            using (var outputCsvFile = File.CreateText(csvPath))
            {
                var writer = new CsvWriter(outputCsvFile);

                // Try writing the mapping for one of the selected datasets
                foreach (var d in datasets.Where(dataset => dataset.Kind == Dataset.DatasetKind.PetResult))
                {
                    if (d.Selected)
                    {
                        if (WriteMappingToCsv(d, writer, dataDirectoryPath))
                            return;
                    }
                }
            }

            // 'mappingPET.csv' should be removed/absent from ./results/ if no mappings were written
            File.Delete(csvPath);
        }

        private static bool WriteMappingToCsv(Dataset d, CsvWriter writer, string dataDirectoryPath)
        {
            Console.WriteLine(d.Name);

            writer.WriteField("VarName");
            writer.WriteField("Type");
            writer.WriteField("Selection");
            writer.NextRecord();

            List<string> names = new List<string>();
            List<string> rangeMins = new List<string>();
            List<string> rangeMaxs = new List<string>();

            foreach (var folder in d.Folders)
            {
                string mdaoName;
                if (d.Kind == Dataset.DatasetKind.PetResult)
                {
                    mdaoName = Path.Combine(dataDirectoryPath, DatasetStore.ResultsDirectory,
                        Regex.Replace(folder, "testbench_manifest\\.json$", "mdao_config.json"));

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
                                    writer.WriteField(variable.Key);

                                    if ((string)variable.Value["type"] == "enum")
                                    {
                                        writer.WriteField("Enumeration");
                                        writer.WriteField(String.Join(",",
                                            Enumerable.Select<JToken, string>(
                                            ((JArray)(variable.Value)["items"]).Children(), (x => x.ToString()))));
                                    }
                                    else
                                    {
                                        //Console.WriteLine(variable.ToString());
                                        writer.WriteField("Numeric");
                                        writer.WriteField((string)((JObject)variable.Value)["RangeMin"] + "," +
                                            (string)((JObject)variable.Value)["RangeMax"]);
                                    }
                                    writer.NextRecord();
                                }
                            }

                            //Console.WriteLine("Names: {0}", names.Count);
                            //Console.WriteLine("rangeMins: {0}", rangeMins.Count);
                            //Console.WriteLine("rangeMaxs: {0}", rangeMaxs.Count);

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

        // Code from MSDN example:  https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}