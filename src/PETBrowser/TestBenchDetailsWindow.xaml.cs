using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AVM.DDP;
using Ookii.Dialogs.Wpf;

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for TestBenchDetailsWindow.xaml
    /// </summary>
    public partial class TestBenchDetailsWindow : Window
    {
        public TBManifestViewModel ViewModel
        {
            get { return (TBManifestViewModel)DataContext; }
            set { DataContext = value; }
        }

        public string ManifestPath { get; private set; }

        public TestBenchDetailsWindow(string manifestPath)
        {
            this.ManifestPath = manifestPath;
            this.ViewModel = new TBManifestViewModel(manifestPath);
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ArtifactsRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (MetaTBManifest.Artifact) ((DataGridRow)sender).Item;

            var containingDirectory = Directory.GetParent(ManifestPath).FullName;
            var artifactPath = System.IO.Path.Combine(containingDirectory, selectedItem.Location);

            try
            {
                Process.Start("explorer.exe", "/select,\"" + artifactPath + "\"");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while opening in Explorer.", "", ex.ToString());
            }
        }

        private void ShowErrorDialog(string title, string mainInstruction, string content, string exceptionDetails)
        {
            var taskDialog = new TaskDialog
            {
                WindowTitle = title,
                MainInstruction = mainInstruction,
                Content = content,
                ExpandedControlText = "Exception details",
                ExpandFooterArea = true,
                ExpandedInformation = exceptionDetails,
                MainIcon = TaskDialogIcon.Error
            };
            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            taskDialog.ShowDialog(this);
            //MessageBox.Show(this, "The selected folder doesn't appear to be a valid data folder.",
            //    "Error loading datasets", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void StepsRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (MetaTBManifest.Step)((DataGridRow)sender).Item;

            var containingDirectory = Directory.GetParent(ManifestPath).FullName;
            var artifactPath = System.IO.Path.Combine(containingDirectory, selectedItem.LogFile.Replace("/", "\\"));

            try
            {
                Process.Start("explorer.exe", "/select,\"" + artifactPath + "\"");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while opening in Explorer.", "", ex.ToString());
            }
        }
    }
}
