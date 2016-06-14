using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AVM.DDP;
using CsvHelper;
using META;

namespace PETBrowser
{
    public class PetDetailsViewModel
    {
        public Dataset DetailsDataset { get; set; }
        public MetaTBManifest Manifest { get; set; }

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
            if (DetailsDataset.Kind == Dataset.DatasetKind.PetResult)
            {
                var datasetPath = System.IO.Path.Combine(resultsDirectory, DetailsDataset.Folders[0]);
                Manifest = MetaTBManifest.Deserialize(datasetPath);
            }
            else
            {
                Manifest = null;
            }
        }

        private void ComputeStatistics()
        {
            foreach (var folder in DetailsDataset.Folders)
            {
                
            }
        }
    }
}
