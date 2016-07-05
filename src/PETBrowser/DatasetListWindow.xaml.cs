using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
            if (datasetToDelete.Kind == Dataset.DatasetKind.Archive)
            {
                var taskDialog = new TaskDialog
                {
                    WindowTitle = "Delete item",
                    MainInstruction = "Are you sure you want to remove this archive?",
                    Content = "This operation cannot be undone.",
                    MainIcon = TaskDialogIcon.Warning
                };

                var deletePermanentlyButton = new TaskDialogButton()
                {
                    ButtonType = ButtonType.Custom,
                    Text = "Remove archive from list",
                    CommandLinkNote =
                        "This archive will be removed from the list and placed in the deleted folder."
                };
                taskDialog.Buttons.Add(deletePermanentlyButton);

                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel)
                {
                    Default = true
                });
                taskDialog.CenterParent = true;
                taskDialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
                var selectedButton = taskDialog.ShowDialog(this);

                if (selectedButton == deletePermanentlyButton)
                {
                    //Delete the item
                    Console.WriteLine("Delete");
                    ViewModel.DeleteItem(datasetToDelete);
                }
            }
            else
            {
                var taskDialog = new TaskDialog
                {
                    WindowTitle = "Delete item",
                    MainInstruction = "Are you sure you want to remove this dataset?",
                    Content = "This operation cannot be undone.",
                    MainIcon = TaskDialogIcon.Warning
                };

                var removeFromListButton = new TaskDialogButton()
                {
                    ButtonType = ButtonType.Custom,
                    Text = "Remove dataset from list",
                    CommandLinkNote =
                        "This dataset will be removed from the list and its files, folders, results, and artifacts will be moved to the deleted folder."
                };
                taskDialog.Buttons.Add(removeFromListButton);

                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel)
                {
                    Default = true
                });
                taskDialog.CenterParent = true;
                taskDialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
                var selectedButton = taskDialog.ShowDialog(this);

                if (selectedButton == removeFromListButton)
                {
                    //Delete the item
                    Console.WriteLine("Remove");
                    ViewModel.DeleteItem(datasetToDelete);
                }
            }
        }

        private void PetSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.FilterPets(PetSearchTextBox.Text);
        }

        private void TestBenchSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.FilterTestBenches(TestBenchSearchTextBox.Text);
        }

        private void CleanupButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Store.Cleanup();
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

        public void FilterPets(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                PetDatasets.Filter = null;
            }
            else
            {
                PetDatasets.Filter = o =>
                {
                    var dataset = (Dataset) o;
                    return ContainsIgnoreCase(dataset.Name, filter);
                };
            }
        }

        public void FilterTestBenches(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                TestBenchDatasets.Filter = null;
            }
            else
            {
                TestBenchDatasets.Filter = o =>
                {
                    var dataset = (Dataset)o;
                    return ContainsIgnoreCase(dataset.Name, filter) || ContainsIgnoreCase(dataset.DesignName, filter); ;
                };
            }
        }

        public void DeleteItem(Dataset datasetToDelete)
        {
            Store.DeleteDataset(datasetToDelete);

            //Naive reload--  this could be faster if we only removed the dataset we deleted
            LoadDataset(Store.DataDirectory);
        }

        private bool ContainsIgnoreCase(string source, string value)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, CompareOptions.IgnoreCase) >= 0;
        }
    }
}
