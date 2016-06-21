using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
            try
            {
                this.ViewModel.LoadDataset(".");
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var promptDialog = new PromptDialog {Owner = this};

                if (promptDialog.ShowDialog() == true)
                {
                    var highlightedDataset = (Dataset)PetGrid.SelectedItem;
                    this.ViewModel.Store.ArchiveSelectedDatasets(promptDialog.Text, highlightedDataset);
                    this.ViewModel.ReloadArchives();
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Archive error", "An error occurred while archiving results.", ex.Message, ex.ToString());
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
            taskDialog.CenterParent = true;
            taskDialog.ShowDialog(this);
            //MessageBox.Show(this, "The selected folder doesn't appear to be a valid data folder.",
            //    "Error loading datasets", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void vizButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var highlightedDataset = (Dataset) PetGrid.SelectedItem;
                var exportPath = this.ViewModel.Store.ExportSelectedDatasetsToViz(highlightedDataset);

                Process.Start(System.IO.Path.Combine(META.VersionInfo.MetaPath, "bin\\Dig\\run.cmd"), exportPath);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Archive error", "An error occurred while archiving results.", ex.Message, ex.ToString());
            }
        }

        private void explorerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDataset = (Dataset) TestBenchGrid.SelectedItem;

                var datasetPath = System.IO.Path.Combine(ViewModel.Store.DataDirectory, DatasetStore.ResultsDirectory,
                    selectedDataset.Folders[0]);

                //Explorer doesn't accept forward slashes in paths
                datasetPath = datasetPath.Replace("/", "\\");

                Console.WriteLine("/select,\"" + datasetPath + "\"");

                Process.Start("explorer.exe", "/select,\"" + datasetPath + "\"");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while opening in Explorer.", "", ex.ToString());
            }
        }

        private void showDetails(object sender, RoutedEventArgs e)
        {
            var selectedDataset = (Dataset)TestBenchGrid.SelectedItem;

            var datasetPath = System.IO.Path.Combine(ViewModel.Store.DataDirectory, DatasetStore.ResultsDirectory,
                selectedDataset.Folders[0]);

            var detailsWindow = new TestBenchDetailsWindow(datasetPath);
            detailsWindow.Owner = this;
            detailsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            detailsWindow.ShowDialog();
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.LoadDataset(ViewModel.Store.DataDirectory);
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

        private void showPetDetails(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDataset = (Dataset) PetGrid.SelectedItem;

                if (selectedDataset.Kind == Dataset.DatasetKind.PetResult)
                {
                    var resultsDirectory = System.IO.Path.Combine(ViewModel.Store.DataDirectory,
                        DatasetStore.ResultsDirectory);

                    var detailsWindow = new PetDetailsWindow(selectedDataset, resultsDirectory, ViewModel);
                    detailsWindow.Owner = this;
                    detailsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    detailsWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while loading dataset details.", ex.Message, ex.ToString());
            }
        }

        private void DeletePetItem(object sender, RoutedEventArgs e)
        {
            var selectedDataset = (Dataset) PetGrid.SelectedItem;
            DeleteItem(selectedDataset);
        }

        private void DeleteTestBenchItem(object sender, RoutedEventArgs e)
        {
            var selectedDataset = (Dataset)TestBenchGrid.SelectedItem;
            DeleteItem(selectedDataset);
        }

        private void DeleteItem(Dataset datasetToDelete)
        {
            var taskDialog = new TaskDialog
            {
                WindowTitle = "Delete item",
                MainInstruction = "Are you sure you want to permanently delete this item?",
                Content = "This operation cannot be undone.",
                ExpandedControlText = "Exception details",
                MainIcon = TaskDialogIcon.Warning
            };
            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Yes));
            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.No)
            {
                Default = true
            });
            taskDialog.CenterParent = true;
            var selectedButton = taskDialog.ShowDialog(this);

            if (selectedButton.ButtonType == ButtonType.Yes)
            {
                //Delete the item
                Console.WriteLine("Delete");
                ViewModel.DeleteItem(datasetToDelete);
            }
        }
    }

    public class DatasetListWindowViewModel
    {
        public List<Dataset> PetDatasetsList { get; set; }
        public ICollectionView PetDatasets { get; set; }

        public List<Dataset> TestBenchDatasetsList { get; set; }
        public ICollectionView TestBenchDatasets { get; set; }

        public DatasetStore Store { get; set; }

        public bool AllowArchive
        {
            get { return PetDatasetsList.Exists(dataset => dataset.Selected); }
        }

        public DatasetListWindowViewModel()
        {
            Store = null;
            PetDatasetsList = new List<Dataset>();
            PetDatasets = new ListCollectionView(PetDatasetsList);

            TestBenchDatasetsList = new List<Dataset>();
            TestBenchDatasets = new ListCollectionView(TestBenchDatasetsList);
        }

        public void LoadDataset(string path)
        {
            Store = new DatasetStore(path);
            PetDatasetsList.Clear();
            PetDatasetsList.AddRange(Store.ResultDatasets);
            PetDatasetsList.AddRange(Store.ArchiveDatasets);
            PetDatasets.Refresh();

            TestBenchDatasetsList.Clear();
            TestBenchDatasetsList.AddRange(Store.TestbenchDatasets);
            TestBenchDatasets.Refresh();
        }

        public void ReloadArchives()
        {
            Store.LoadArchiveDatasets();

            PetDatasetsList.Clear();
            PetDatasetsList.AddRange(Store.ResultDatasets);
            PetDatasetsList.AddRange(Store.ArchiveDatasets);
            PetDatasets.Refresh();
        }

        public void DeleteItem(Dataset datasetToDelete)
        {
            Store.DeleteDataset(datasetToDelete);

            //Naive reload--  this could be faster if we only removed the dataset we deleted
            LoadDataset(Store.DataDirectory);
        }
    }
}
