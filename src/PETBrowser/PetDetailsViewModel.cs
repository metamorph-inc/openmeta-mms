using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public class PetDetailsViewModel
    {
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

        public Dataset DetailsDataset { get; set; }
        public MetaTBManifest Manifest { get; set; }
        public string MgaFilename { get; set; }
        public string MgaFilePath { get; set; }
        public int RecordCount { get; set; }
        public ICollectionView Metrics { get; set; }
        private string ResultsDirectory { get; set; }

        public string CreatedTime
        {
            get
            {
                var parsedTime = DateTime.Parse(Manifest.Created);
                return parsedTime.ToString("G");
            }
        }

        public PetDetailsViewModel(Dataset dataset, string resultsDirectory)
        {
            DetailsDataset = dataset;
            ResultsDirectory = resultsDirectory;
            RecordCount = 0;
            MgaFilename = "";
            MgaFilePath = "";
            if (DetailsDataset.Kind == Dataset.DatasetKind.PetResult)
            {
                var datasetPath = System.IO.Path.Combine(resultsDirectory, DetailsDataset.Folders[0]);
                Manifest = MetaTBManifest.Deserialize(datasetPath);

                try
                {
                    var jsonFileName = Path.Combine(ResultsDirectory,
                        DetailsDataset.Folders[0].Replace("testbench_manifest.json", "mdao_config.json"));

                    using (var reader = File.OpenText(jsonFileName))
                    {
                        var serializer = new JsonSerializer();
                        var mdaoConfig = (PETConfig) serializer.Deserialize(reader, typeof(PETConfig));

                        if (!string.IsNullOrEmpty(mdaoConfig.MgaFilename))
                        {
                            MgaFilePath = mdaoConfig.MgaFilename;
                            MgaFilename = Path.GetFileName(MgaFilePath);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occurred reading mdao_config.json: ");
                    Console.WriteLine(e);

                    Trace.TraceError("Error occurred reading mdao_config.json: ");
                    Trace.TraceError(e.ToString());
                }

                ComputeStatistics();
            }
            else
            {
                Manifest = null;
            }
        }

        private void ComputeStatistics()
        {
            Dictionary<string, Metric> metrics = new Dictionary<string, Metric>();
            foreach (var folder in DetailsDataset.Folders)
            {
                Dictionary<string, Metric.MetricKind> metricKinds = new Dictionary<string, Metric.MetricKind>();

                try
                {
                    var jsonFileName = Path.Combine(ResultsDirectory,
                        folder.Replace("testbench_manifest.json", "mdao_config.json"));

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
                    var csvFileName = Path.Combine(ResultsDirectory,
                        folder.Replace("testbench_manifest.json", "output.csv"));

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
    }
}
