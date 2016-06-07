using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PETBrowser
{
    public class PetDetailsViewModel
    {
        public Dataset DetailsDataset { get; set; }

        public PetDetailsViewModel(Dataset dataset)
        {
            DetailsDataset = dataset;
        }
    }
}
