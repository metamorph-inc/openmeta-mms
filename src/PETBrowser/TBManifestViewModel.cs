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
            VisualizationArtifacts = new ListCollectionView(Manifest.VisualizationArtifacts);
            Metrics = new ListCollectionView(Manifest.Metrics);
            Parameters = new ListCollectionView(Manifest.Parameters);
            Steps = new ListCollectionView(Manifest.Steps);
        }
    }
}