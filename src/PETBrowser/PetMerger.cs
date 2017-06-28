using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AVM.DDP;
using CsvHelper;
using CsvHelper.Configuration;
using JobManager;
using JobManagerFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PETBrowser
{
    public class MergedPetMetadata
    {
        public enum MergedPetKind
        {
            MergedPet = 0,
            AutomaticPet = 1
        }

        [DefaultValue(MergedPetKind.MergedPet)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public MergedPetKind Kind { get; set; }

        public List<Dataset> SourceDatasets { get; set; }

        public MergedPetMetadata()
        {
            SourceDatasets = new List<Dataset>();
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

        [JsonProperty(PropertyName = "design_tree")]
        public string DesignTree { get; set; }

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
                var exportPath = Path.Combine(tempDirectoryPath, "mergedPET.csv");
                var mergedPetConfigPath = Path.Combine(tempDirectoryPath, "pet_config.json");

                WriteSelectedDatasetsToCsv(exportPath, true, datasets, dataDirectoryPath, true, true, true);
                WriteSummarizedPetConfig(mergedPetConfigPath, datasets, dataDirectoryPath);
                MergeDesignTrees(tempDirectoryPath, datasets, dataDirectoryPath, null);

                BuildSkeletonMergeDirectory(tempDirectoryPath, datasets, MergedPetMetadata.MergedPetKind.MergedPet);

                DirectoryCopy(tempDirectoryPath, mergedPetDirectory, true);
            }
            finally
            {
                // Make sure our temp directory is removed, whether we complete successfully or an error occurs
                Directory.Delete(tempDirectoryPath, true);
            }
        }

        public static void RefreshMergedPet(Dataset datasetToRefresh, string dataDirectoryPath)
        {
            if (datasetToRefresh.Kind != Dataset.DatasetKind.MergedPet && datasetToRefresh.Kind != Dataset.DatasetKind.Pet)
            {
                throw new InvalidOperationException("Only MergedPet datasets can be refreshed.");
            }

            var mergedPetDirectory = Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory,
                datasetToRefresh.Folders[0]);
            Console.WriteLine("Merged PET directory: {0}", mergedPetDirectory);
            Console.WriteLine("Dataset Folder: {0}", datasetToRefresh.Folders[0]);

            var datasets = new List<Dataset>();

            MergedPetMetadata.MergedPetKind kind;

            using (
                var reader =
                    File.OpenText(Path.Combine(mergedPetDirectory, "metadata.json")))
            {
                var serializer = new JsonSerializer();
                var metadata = (MergedPetMetadata) serializer.Deserialize(reader, typeof(MergedPetMetadata));
                kind = metadata.Kind;

                datasets.AddRange(metadata.SourceDatasets);
            }

            var tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDirectoryPath);

            try
            {
                var exportPath = Path.Combine(tempDirectoryPath, "mergedPET.csv");
                var mergedPetConfigPath = Path.Combine(tempDirectoryPath, "pet_config.json");
                var finalVizConfigPath = Path.Combine(mergedPetDirectory, "visualizer_config.json");
                var finalDesignTreePath = Path.Combine(mergedPetDirectory, "design_tree.json");

                bool writeVizConfig = !File.Exists(finalVizConfigPath); // Don't overwrite viz config if it already exists

                WriteSelectedDatasetsToCsv(exportPath, true, datasets, dataDirectoryPath, true, true, true);
                WriteSummarizedPetConfig(mergedPetConfigPath, datasets, dataDirectoryPath);
                MergeDesignTrees(tempDirectoryPath, datasets, dataDirectoryPath, ReadDesignTree(finalDesignTreePath));

                BuildSkeletonMergeDirectory(tempDirectoryPath, datasets, kind, writeVizConfig);

                DirectoryCopy(tempDirectoryPath, mergedPetDirectory, true, true);
            }
            finally
            {
                // Make sure our temp directory is removed, whether we complete successfully or an error occurs
                Directory.Delete(tempDirectoryPath, true);
            }
        }

        public static void BuildPetDirectory(JobCollection jobCollection)
        {
            if (!(jobCollection is JobServerImpl.JobCollectionImpl))
            {
                throw new ArgumentException("Invalid job collection object");
            }
            var impl = (JobServerImpl.JobCollectionImpl) jobCollection;

            if (impl.Jobs.Count == 0)
            {
                return;
            }

            var firstJob = impl.Jobs[0];
            var isPet = File.Exists(Path.Combine(firstJob.WorkingDirectory, "mdao_config.json")); //TODO: Is there a better heuristic to identify a PET?

            if (isPet)
            {
                var baseDataDirectory =
                    Path.GetFullPath(Path.Combine(firstJob.WorkingDirectory, "..",
                        "..")); //TODO: Need a better way to get the project directory for a PET
                Console.WriteLine("PET added: base directory: {0}", baseDataDirectory);

                var dataset = new Dataset(Dataset.DatasetKind.PetResult, "", Guid.NewGuid().ToString());
                dataset.Selected = true;
                foreach (var job in impl.Jobs)
                {
                    var jobWorkingDirectory = Path.GetFileName(job.WorkingDirectory);
                    var jobDatasetPath = Path.Combine(".", jobWorkingDirectory, "testbench_manifest.json");
                    dataset.Folders.Add(jobDatasetPath);
                }
                var datasets = new Dataset[] {dataset};

                var mergedDirectory = Path.Combine(baseDataDirectory, MergedDirectory);

                if (!Directory.Exists(mergedDirectory))
                {
                    Directory.CreateDirectory(mergedDirectory);
                }

                var tempDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDirectoryPath);

                try
                {
                    BuildSkeletonMergeDirectory(tempDirectoryPath, datasets, MergedPetMetadata.MergedPetKind.AutomaticPet);

                    string designTreePath = Path.Combine(tempDirectoryPath, "design_tree.json");
                    WriteDesignTree(designTreePath, jobCollection.Designs);

                    var mergedName = GetMergedPetName(firstJob.TestBenchName, mergedDirectory); //Possible race condition here
                    var mergedPetDirectory = Path.Combine(mergedDirectory, mergedName);

                    if (Directory.Exists(mergedPetDirectory))
                    {
                        throw new InvalidOperationException("A merged PET with this name already exists."); // TODO: use a better exception here
                    }

                    DirectoryCopy(tempDirectoryPath, mergedPetDirectory, true);
                }
                finally
                {
                    // Make sure our temp directory is removed, whether we complete successfully or an error occurs
                    Directory.Delete(tempDirectoryPath, true);
                }
            }
            else
            {
                Console.WriteLine("Job added that isn't a PET");
            }
        }

        private static string GetMergedPetName(string testBenchName, string mergedDirectory)
        {
            if (!Directory.Exists(Path.Combine(mergedDirectory, testBenchName)))
            {
                return testBenchName;
            }
            else
            {
                for (int i = 1; ; i++)
                {
                    var candidateName = string.Format("{0} ({1})", testBenchName, i);
                    if (!Directory.Exists(Path.Combine(mergedDirectory, candidateName)))
                    {
                        return candidateName;
                    }
                }
            }
        }

        private static void BuildSkeletonMergeDirectory(string directoryPath, IEnumerable<Dataset> datasets, MergedPetMetadata.MergedPetKind kind, bool writeVizConfig = true)
        {
            var metadataPath = Path.Combine(directoryPath, "metadata.json");
            var vizConfigPath = Path.Combine(directoryPath, "visualizer_config.json");

            WriteMergedMetadata(metadataPath, datasets, kind);
            if (writeVizConfig)
            {
                WriteDefaultVizConfig(vizConfigPath);
            }
        }

        private static void WriteMergedMetadata(string filePath, IEnumerable<Dataset> datasets, MergedPetMetadata.MergedPetKind kind)
        {
            var metadata = new MergedPetMetadata();

            metadata.Kind = kind;
            metadata.SourceDatasets.AddRange(datasets);

            using (var writer = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, metadata);
            }
        }

        public static void WriteDefaultVizConfig(string vizConfigPath)
        {
            var config = new VisualizerConfig();
            config.PetConfig = "pet_config.json";
            config.RawData = "mergedPET.csv";
            config.Tabs = new List<string> { "Explore.R", "DataTable.R", "PETRefinement.R" };
            config.DesignTree = "design_tree.json";

            using (var writer = File.CreateText(vizConfigPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, config);
            }
        }

        private static void WriteDesignTree(string designTreeJsonPath, Dictionary<string, MetaTBManifest.DesignType> designs)
        {
            using (var writer = File.CreateText(designTreeJsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, designs);
            }
        }

        private static void MergeDesignTrees(string mergedDirectoryPath, List<Dataset> datasets, string dataDirectoryPath, Dictionary<string, MetaTBManifest.DesignType> originalDesignTree)
        {
            Dictionary<string, MetaTBManifest.DesignType> mergedDesignTree;
            if (originalDesignTree != null)
            {
                mergedDesignTree = originalDesignTree;
            }
            else
            {
                mergedDesignTree = new Dictionary<string, MetaTBManifest.DesignType>();
            }

            foreach (var dataset in datasets)
            {
                if (dataset.Kind == Dataset.DatasetKind.Pet || dataset.Kind == Dataset.DatasetKind.MergedPet)
                {
                    var datasetDesignTreeJsonPath =
                        Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory, dataset.Folders[0],
                            "design_tree.json");

                    var datasetDesignTree = ReadDesignTree(datasetDesignTreeJsonPath);

                    foreach (var key in datasetDesignTree.Keys)
                    {
                        mergedDesignTree[key] = datasetDesignTree[key];
                    }
                }
            }

            var mergedDesignTreePath = Path.Combine(mergedDirectoryPath, "design_tree.json");
            WriteDesignTree(mergedDesignTreePath, mergedDesignTree);
        }

        private static Dictionary<string, MetaTBManifest.DesignType> ReadDesignTree(string path)
        {
            if (File.Exists(path))
            {
                using (var reader = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var designTree = (Dictionary<string, MetaTBManifest.DesignType>) serializer.Deserialize(reader,
                        typeof(Dictionary<string, MetaTBManifest.DesignType>));
                    return designTree;
                }
            }
            else
            {
                //Design tree doesn't exist; return an empty one
                return new Dictionary<string, MetaTBManifest.DesignType>();
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
                } else if (d.Kind == Dataset.DatasetKind.MergedPet || d.Kind == Dataset.DatasetKind.Pet)
                {
                    csvFileName = Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory, folder, "mergedPET.csv");
                }
                else
                {
                    csvFileName = Path.Combine(dataDirectoryPath, DatasetStore.ArchiveDirectory, folder);
                }

                Console.WriteLine("Exporting CSV {0}", csvFileName);

                //Assume the added columns don't exist in the data we're reading from 
                foreach (var header in addedHeaders)
                    headersPresent[header.Key] = false;

                try
                {
                    using (var csvFile = new StreamReader(File.Open(csvFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
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
                }
                catch (CsvReaderException e)
                {
                    Console.WriteLine("Invalid CSV found at {0}", csvFileName);
                    Trace.TraceWarning("Invalid CSV found at {0}", csvFileName);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("Missing CSV at {0}", csvFileName);
                    Trace.TraceWarning("Missing CSV at {0}", csvFileName);
                }
            }
        }

        private static void WriteSummarizedPetConfig(string mergedPetConfigPath, IEnumerable<Dataset> datasets, string dataDirectoryPath)
        {
            //Compute the list of mdao_configs to summarize
            List<string> petConfigPaths = new List<string>();

            foreach (var d in datasets)
            {
                if (d.Kind == Dataset.DatasetKind.PetResult)
                {
                    foreach (var folder in d.Folders)
                    {
                        petConfigPaths.Add(Path.Combine(dataDirectoryPath, DatasetStore.ResultsDirectory,
                            folder.Replace("testbench_manifest.json", "mdao_config.json")));
                    }
                }
                else if(d.Kind == Dataset.DatasetKind.MergedPet || d.Kind == Dataset.DatasetKind.Pet)
                {
                    var path = Path.Combine(dataDirectoryPath, DatasetStore.MergedDirectory, d.Folders[0], "pet_config.json");

                    if (File.Exists(path))
                    {
                        petConfigPaths.Add(path);
                    }
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
                        var petConfig = (PETConfig) serializer.Deserialize(configReader, typeof(PETConfig));

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
                catch (FileNotFoundException e)
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
        
        // Code from MSDN example:  https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = false)
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
                file.CopyTo(temppath, overwrite);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwrite);
                }
            }
        }

        public static void RenameMergedPet(Dataset dataset, string newName, string dataDirectory)
        {
            var originalPath = Path.Combine(dataDirectory, MergedDirectory, dataset.Name);
            var newPath = Path.Combine(dataDirectory, MergedDirectory, newName);

            if (Directory.Exists(newPath))
            {
                throw new ArgumentException("A merged PET by this name already exists.");
            }

            Directory.Move(originalPath, newPath);
        }
    }
}