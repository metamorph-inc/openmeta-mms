using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using AVM.DDP;
using CsvHelper;
using CsvHelper.Configuration;
using META;
using Newtonsoft.Json;

namespace PETBrowser
{
    public class MergedPetDetailsViewModel : IDisposable
    {
        private static readonly HashSet<string> IgnoredMetricNames = new HashSet<string>(new []
        {
            "CfgID",
            "GUID",
            "DesignContainer"
        });

        public class Metric
        {
            public enum MetricDataType
            {
                Number,
                String
            }

            public enum MetricKind
            {
                DesignVariable,
                Objective,
                Constraint,
                IntermediateVariable,
                Unknown
            }
            public string Name { get; set; }
            public MetricDataType DataType { get; set; }
            public MetricKind Kind { get; set; }
            public double Min { get; set; }

            public string MinFormatted
            {
                get { return DataType == MetricDataType.Number ? Min.ToString() : "N/A"; }
            }

            public double Max { get; set; }

            public string MaxFormatted
            {
                get { return DataType == MetricDataType.Number ? Max.ToString(): "N/A"; }
            }
            public double Sum { get; set; }
            public int Count { get; set; }

            public string AverageFormatted
            {
                get {
                    if (DataType == MetricDataType.String || Count == 0)
                    {
                        return "N/A";
                    }
                    else
                    {
                        return (Sum/Count).ToString();
                    }
                }
            }

            public Metric(string name)
            {
                Name = name;
                DataType = MetricDataType.Number;
                Kind = MetricKind.Unknown;
                Min = double.MaxValue;
                Max = double.MinValue;
                Sum = 0;
                Count = 0;
            }
        }

        public class VisualizerSession : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string DisplayName { get; private set; }
            public string ConfigPath { get; private set; }
            public DateTime DateModified { get; private set; }

            private bool _visualizerNotRunning;

            public bool VisualizerNotRunning
            {
                get { return _visualizerNotRunning; }
                set
                {
                    PropertyChanged.ChangeAndNotify(ref _visualizerNotRunning, value, () => VisualizerNotRunning);
                }
            }

            public VisualizerSession(string displayName, string configPath, DateTime dateModified, bool visualizerNotRunning)
            {
                DisplayName = displayName;
                ConfigPath = configPath;
                DateModified = dateModified;
                VisualizerNotRunning = visualizerNotRunning;
            }
        }

        public Dataset DetailsDataset { get; set; }
        public string MgaFilename { get; set; }
        public string MgaFilePath { get; set; }
        public string PetPath { get; set; }
        public int RecordCount { get; set; }
        public ICollectionView Metrics { get; set; }
        public List<VisualizerSession> VisualizerSessionsList { get; set; }
        public ICollectionView VisualizerSessions { get; set; }
        private string MergedDirectory { get; set; }

        public string CreatedTime
        {
            get
            {
                var parsedTime = DateTime.ParseExact(DetailsDataset.Time, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);
                return parsedTime.ToString("G");
            }
        }

        public MergedPetDetailsViewModel(Dataset dataset, string mergedDirectory)
        {
            DetailsDataset = dataset;
            MergedDirectory = mergedDirectory;
            RecordCount = 0;
            MgaFilename = "";
            MgaFilePath = "";
            PetPath = "";
            if (DetailsDataset.Kind == Dataset.DatasetKind.MergedPet || DetailsDataset.Kind == Dataset.DatasetKind.Pet)
            {
                var datasetPath = System.IO.Path.Combine(mergedDirectory, DetailsDataset.Folders[0]);

                try
                {
                    var jsonFileName = Path.Combine(MergedDirectory, DetailsDataset.Folders[0], "pet_config.json");

                    using (var reader = File.OpenText(jsonFileName))
                    {
                        var serializer = new JsonSerializer();
                        var mdaoConfig = (PETConfig) serializer.Deserialize(reader, typeof(PETConfig));

                        if (!string.IsNullOrEmpty(mdaoConfig.MgaFilename))
                        {
                            MgaFilePath = mdaoConfig.MgaFilename;
                            MgaFilename = Path.GetFileName(MgaFilePath);
                        }

                        if (!string.IsNullOrEmpty(mdaoConfig.PETName))
                        {
                            PetPath = mdaoConfig.PETName;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occurred reading pet_config.json: ");
                    Console.WriteLine(e);

                    Trace.TraceError("Error occurred reading pet_config.json: ");
                    Trace.TraceError(e.ToString());
                }

                ComputeStatistics();
                VisualizerSessionsList = GetVisualizerSessions(mergedDirectory);
                VisualizerSessions = new ListCollectionView(VisualizerSessionsList);
                VisualizerSessions.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

                VisualizerLauncher.VisualizerExited += OnVisualizerExited;
            }
            else
            {
            }
        }

        private void OnVisualizerExited(object sender, VisualizerLauncher.VisualizerExitedEventArgs visualizerExitedEventArgs)
        {
            Console.WriteLine("OnVisualizerExited");
            VisualizerSessionsList.FindAll(session => session.ConfigPath == visualizerExitedEventArgs.ConfigPath).ForEach(session => session.VisualizerNotRunning = true);
        }

        public string CreateNewVisualizerSession(string name)
        {
            var baseDirectory = Path.Combine(MergedDirectory, DetailsDataset.Folders[0]);
            var newSessionFileName = string.Format("{0}-visualizer_config.json", name);
            var newSessionPath = Path.Combine(baseDirectory, newSessionFileName);

            if (File.Exists(newSessionPath))
            {
                throw new InvalidOperationException("A visualizer session with this name already exists.");
            }

            PetMerger.WriteDefaultVizConfig(newSessionPath);

            //Reload visualizer session list
            VisualizerSessionsList.Clear();
            VisualizerSessionsList.AddRange(GetVisualizerSessions(MergedDirectory));
            VisualizerSessions.Refresh();

            return newSessionPath;
        }

        private List<VisualizerSession> GetVisualizerSessions(string mergedDirectory)
        {
            var result = new List<VisualizerSession>();

            var baseDirectory = Path.Combine(mergedDirectory, DetailsDataset.Folders[0]);

            var defaultVizConfigPath = Path.Combine(baseDirectory, "visualizer_config.json");

            if (File.Exists(defaultVizConfigPath))
            {
                var dateModified = File.GetLastWriteTime(defaultVizConfigPath);
                var visualizerRunning = VisualizerLauncher.IsVisualizerRunningForConfig(defaultVizConfigPath);
                result.Add(new VisualizerSession("Default", defaultVizConfigPath, dateModified, !visualizerRunning));
            }

            var vizConfigFiles = Directory.EnumerateFiles(baseDirectory, "*-visualizer_config.json");

            foreach (var vizConfigPath in vizConfigFiles)
            {
                Console.WriteLine(vizConfigPath);
                var vizFileName = Path.GetFileName(vizConfigPath);
                var sessionName = vizFileName.Remove(vizFileName.LastIndexOf("-visualizer_config.json", StringComparison.Ordinal));
                Console.WriteLine(sessionName);

                var dateModified = File.GetLastWriteTime(vizConfigPath);
                var visualizerRunning = VisualizerLauncher.IsVisualizerRunningForConfig(vizConfigPath);
                result.Add(new VisualizerSession(sessionName, vizConfigPath, dateModified, !visualizerRunning));
            }

            return result;
        }

        private void ComputeStatistics()
        {
            Dictionary<string, Metric> metrics = new Dictionary<string, Metric>();
            foreach (var folder in DetailsDataset.Folders)
            {
                Dictionary<string, Metric.MetricKind> metricKinds = new Dictionary<string, Metric.MetricKind>();

                try
                {
                    var jsonFileName = Path.Combine(MergedDirectory, folder, "pet_config.json");

                    using (var reader = File.OpenText(jsonFileName))
                    {
                        var serializer = new JsonSerializer();
                        var mdaoConfig = (PETConfig) serializer.Deserialize(reader, typeof(PETConfig));

                        foreach (var driver in mdaoConfig.drivers)
                        {
                            foreach (var designVar in driver.Value.designVariables)
                            {
                                metricKinds[designVar.Key] = Metric.MetricKind.DesignVariable;
                            }

                            foreach (var objective in driver.Value.objectives)
                            {
                                metricKinds[objective.Key] = Metric.MetricKind.Objective;
                            }

                            foreach (var constraint in driver.Value.constraints)
                            {
                                metricKinds[constraint.Key] = Metric.MetricKind.Constraint;
                            }

                            foreach (var intermediateVar in driver.Value.intermediateVariables)
                            {
                                metricKinds[intermediateVar.Key] = Metric.MetricKind.IntermediateVariable;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Silently ignore mdao_config files that don't exist or can't be read
                    Console.WriteLine(e);
                }

                try
                {
                    var csvFileName = Path.Combine(MergedDirectory, DetailsDataset.Folders[0], "mergedPET.csv");

                    using (var csvFile = new StreamReader(File.Open(csvFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
                    {
                        var csvReader = new CsvReader(csvFile, new CsvConfiguration()
                        {
                            HasHeaderRecord = true
                        });

                        while (csvReader.Read())
                        {
                            RecordCount++;
                            foreach (var header in csvReader.FieldHeaders)
                            {
                                if (IgnoredMetricNames.Contains(header))
                                {
                                    continue;
                                }
                                string fieldValue = csvReader.GetField<string>(header);

                                if (fieldValue == "" || fieldValue == "None")
                                {
                                    // Ignore empty/none values
                                }
                                else
                                {
                                    if (!metrics.ContainsKey(header))
                                    {
                                        metrics[header] = new Metric(header);

                                        if (metricKinds.ContainsKey(header))
                                        {
                                            metrics[header].Kind = metricKinds[header];
                                        }
                                    }

                                    var thisMetric = metrics[header];

                                    if (thisMetric.DataType == Metric.MetricDataType.Number)
                                    {
                                        double doubleValue = 0;
                                        var isDouble = double.TryParse(fieldValue, out doubleValue);

                                        if (isDouble)
                                        {
                                            thisMetric.Sum += doubleValue;
                                            thisMetric.Count++;

                                            if (doubleValue > thisMetric.Max)
                                            {
                                                thisMetric.Max = doubleValue;
                                            }
                                            if (doubleValue < thisMetric.Min)
                                            {
                                                thisMetric.Min = doubleValue;
                                            }
                                        }
                                        else
                                        {
                                            thisMetric.DataType = Metric.MetricDataType.String;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Silently ignore CSV files that don't exist or can't be read
                    Console.WriteLine(e);
                }
                
            }

            var metricsList = new List<Metric>(metrics.Values);
            Metrics = new ListCollectionView(metricsList);
            Metrics.GroupDescriptions.Add(new PropertyGroupDescription("Kind"));
            Metrics.SortDescriptions.Add(new SortDescription("Kind", ListSortDirection.Ascending));
            Metrics.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        public void Dispose()
        {
            VisualizerLauncher.VisualizerExited -= OnVisualizerExited;
        }
    }
}
