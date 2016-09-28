using System;
using System.ComponentModel;
using System.Windows.Data;
using AVM.DDP;

namespace PETBrowser
{
    public class TBManifestViewModel
    {
        public MetaTBManifest Manifest { get; set; }

        public ICollectionView Dependencies { get; set; }

        public ICollectionView Artifacts { get; set; }

        public ICollectionView VisualizationArtifacts { get; set; }

        public ICollectionView Metrics { get; set; }

        public ICollectionView Parameters { get; set; }

        public ICollectionView Steps { get; set; }

        public string CreatedTime
        {
            get
            {
                var parsedTime = DateTime.Parse(Manifest.Created);
                return parsedTime.ToString("G");
            }
        }

        public TBManifestViewModel(string manifestPath)
        {
            Manifest = MetaTBManifest.Deserialize(manifestPath);

            Dependencies = new ListCollectionView(Manifest.Dependencies);
            Artifacts = new ListCollectionView(Manifest.Artifacts);
            Artifacts.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Ascending));
            VisualizationArtifacts = new ListCollectionView(Manifest.VisualizationArtifacts);
            VisualizationArtifacts.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Ascending));
            Metrics = new ListCollectionView(Manifest.Metrics);
            Metrics.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Parameters = new ListCollectionView(Manifest.Parameters);
            Parameters.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Steps = new ListCollectionView(Manifest.Steps);
        }
    }
}