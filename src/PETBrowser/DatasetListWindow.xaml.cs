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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Policy;
using Ookii.Dialogs.Wpf;

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for DatasetListWindow.xaml
    /// </summary>
    public partial class DatasetListWindow : Window
    {
        public DatasetListWindowViewModel ViewModel
        {
            get { return (DatasetListWindowViewModel) DataContext; }
            set { DataContext = value; }
        }

        public DatasetListWindow()
        {
            this.ViewModel = new DatasetListWindowViewModel();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.ViewModel.LoadDataset("C:\\source\\viz");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var promptDialog = new PromptDialog {Owner = this};

                if (promptDialog.ShowDialog() == true)
                {
                    this.ViewModel.Store.ArchiveSelectedDatasets(promptDialog.Text);
                    this.ViewModel.ReloadArchives();
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Archive error", "An error occurred while archiving results.", "", ex.ToString());
            }
        }

        private void SelectResultsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;

            if (folderDialog.ShowDialog() == true)
            {
                try
                {
                    ViewModel.LoadDataset(folderDialog.SelectedPath);
                }
                catch (FileNotFoundException ex)
                {
                    ShowErrorDialog("Error loading datasets", "The selected folder doesn't appear to be a valid data folder.", "Make sure the selected folder contains a \"results\" folder with a \"results.metaresults.json\" file within it.", ex.ToString());
                }
                catch (DirectoryNotFoundException ex)
                {
                    ShowErrorDialog("Error loading datasets", "The selected folder doesn't appear to be a valid data folder.", "Make sure the selected folder contains a \"results\" folder with a \"results.metaresults.json\" file within it.", ex.ToString());
                }
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

        private void vizButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportPath = this.ViewModel.Store.ExportSelectedDatasetsToViz();

                Process.Start("C:\\Program Files (x86)\\META\\bin\\Dig\\run.cmd", exportPath);

                var taskDialog = new TaskDialog
                {
                    WindowTitle = "Export Succeeded",
                    MainInstruction = "The export completed successfully.",
                    Content = "Results have been placed in results\\mergedPET.csv for use with the visualizer.  Launch the visualizer to continue.",
                    MainIcon = TaskDialogIcon.Information
                };
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
                taskDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Archive error", "An error occurred while archiving results.", "", ex.ToString());
            }
        }
    }

    public class DatasetListWindowViewModel
    {
        public List<Dataset> DatasetsList { get; set; }
        public ICollectionView Datasets { get; set; }
        public DatasetStore Store { get; set; }

        public bool AllowArchive
        {
            get { return DatasetsList.Exists(dataset => dataset.Selected); }
        }

        public DatasetListWindowViewModel()
        {
            Store = null;
            DatasetsList = new List<Dataset>();
            Datasets = new ListCollectionView(DatasetsList);
        }

        public void LoadDataset(string path)
        {
            Store = new DatasetStore(path);
            DatasetsList.Clear();
            DatasetsList.AddRange(Store.ResultDatasets);
            DatasetsList.AddRange(Store.ArchiveDatasets);
            Datasets.Refresh();
        }

        public void ReloadArchives()
        {
            Store.LoadArchiveDatasets();

            DatasetsList.Clear();
            DatasetsList.AddRange(Store.ResultDatasets);
            DatasetsList.AddRange(Store.ArchiveDatasets);
            Datasets.Refresh();
        }
    }
}
