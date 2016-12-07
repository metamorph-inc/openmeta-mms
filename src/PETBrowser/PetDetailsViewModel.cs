using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using AVM.DDP;
using CsvHelper;
using CsvHelper.Configuration;
using META;

namespace PETBrowser
{
    public class PetDetailsViewModel
    {
        public class Metric
        {
            public enum MetricKind
            {
                Number,
                String
            }
            public string Name { get; set; }
            public MetricKind Kind { get; set; }
            public double Min { get; set; }

            public string MinFormatted
            {
                get { return Kind == MetricKind.Number ? Min.ToString() : "N/A"; }
            }

            public double Max { get; set; }

            public string MaxFormatted
            {
                get { return Kind == MetricKind.Number ? Max.ToString(): "N/A"; }
            }
            public double Sum { get; set; }
            public int Count { get; set; }

            public string AverageFormatted
            {
                get {
                    if (Kind == MetricKind.String || Count == 0)
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
                Kind = MetricKind.Number;
                Min = double.MaxValue;
                Max = double.MinValue;
                Sum = 0;
                Count = 0;
            }
        }

        public Dataset DetailsDataset { get; set; }
        public MetaTBManifest Manifest { get; set; }
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
            if (DetailsDataset.Kind == Dataset.DatasetKind.PetResult)
            {
                var datasetPath = System.IO.Path.Combine(resultsDirectory, DetailsDataset.Folders[0]);
                Manifest = MetaTBManifest.Deserialize(datasetPath);
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
                                    }

                                    var thisMetric = metrics[header];

                                    if (thisMetric.Kind == Metric.MetricKind.Number)
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
                                            thisMetric.Kind = Metric.MetricKind.String;
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
            Metrics.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }
    }
}
