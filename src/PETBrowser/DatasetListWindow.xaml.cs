using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using JobManager;
using Ookii.Dialogs.Wpf;

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for DatasetListWindow.xaml
    /// </summary>
    public partial class DatasetListWindow : Window
    {
        // PInvoke declarations for native window manipulation (specifically, setting the "disabled" style)
        const int GWL_STYLE = -16;
        const int WS_DISABLED = 0x08000000;
        
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private string initialWorkingDirectory;

        private SingleInstanceManager instanceManager;

        void SetNativeEnabled(bool enabled)
        {
            var windowHandle = new WindowInteropHelper(this).Handle;
            SetWindowLong(windowHandle, GWL_STYLE, GetWindowLong(windowHandle, GWL_STYLE) &
                ~WS_DISABLED | (enabled ? 0 : WS_DISABLED));
        }

        public DatasetListWindowViewModel ViewModel
        {
            get { return (DatasetListWindowViewModel) DataContext; }
            set { DataContext = value; }
        }

        public DatasetListWindow() : this(null, null, ".")
        {
        }

        public DatasetListWindow(JobStore jobStore, SingleInstanceManager instanceManager, string initialWorkingDirectory)
        {
            try
            {
                this.initialWorkingDirectory = initialWorkingDirectory;

                this.ViewModel = new DatasetListWindowViewModel(jobStore);

                if (instanceManager == null)
                {
                    this.instanceManager = new SingleInstanceManager();
                    this.instanceManager.OnCreateForWorkingDirectory += (sender, args) =>
                    {
                        Task.Factory.StartNew(() =>
                        {
                            var newDatasetListWindow = new DatasetListWindow(ViewModel.JobStore, this.instanceManager, args.WorkingDirectory);
                            newDatasetListWindow.Show();
                        }, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                    };
                }
                else
                {
                    this.instanceManager = instanceManager;
                }

                InitializeComponent();
                this.ViewModel.TrackedJobsChanged += (sender, args) =>
                {
                    TabControl.SelectedItem = JobsTab;
                };
                Console.WriteLine("Results browser window opened");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Trace.TraceError("Error occurred while creating window:");
                Trace.TraceError(e.ToString());
                throw e;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PetDetailsPanel.Children.Add(new PlaceholderDetailsPanel());
            TestBenchDetailsPanel.Children.Add(new PlaceholderDetailsPanel());
            //this.ViewModel.LoadDataset("C:\\source\\viz");
            LoadDataset(initialWorkingDirectory);
        }

        private void LoadDataset(string path)
        {
            this.ViewModel.LoadDataset(path, exception =>
            {
                if (exception is FileNotFoundException || exception is DirectoryNotFoundException)
                {
                    ShowErrorDialog("Error loading datasets",
                        "The selected folder doesn't appear to be a valid data folder.",
                        "Make sure the selected folder contains a \"results\" folder with a \"results.metaresults.json\" file within it.",
                        exception.ToString());
                }
                else
                {
                    ShowErrorDialog("Error loading datasets",
                        "An unknown error occurred while loading datasets.",
                        "Make sure the selected folder contains a \"results\" folder with a \"results.metaresults.json\" file within it.",
                        exception.ToString());
                }
            });
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
                LoadDataset(folderDialog.SelectedPath);
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
                string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "OpenMETA_Visualizer_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = String.Format("/S /C \"\"{0}\" \"{1}\" > \"{2}\" 2>&1\"", System.IO.Path.Combine(META.VersionInfo.MetaPath, "bin\\Dig\\run.cmd"), exportPath, logPath),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    // WorkingDirectory = ,
                    // RedirectStandardError = true,
                    // RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var p = new Process();
                p.StartInfo = psi;
                p.Start();

                p.Dispose();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while starting visualizer.", ex.Message, ex.ToString());
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
            /*var selectedDataset = (Dataset)TestBenchGrid.SelectedItem;

            var datasetPath = System.IO.Path.Combine(ViewModel.Store.DataDirectory, DatasetStore.ResultsDirectory,
                selectedDataset.Folders[0]);

            var detailsWindow = new TestBenchDetailsControl(datasetPath);
            detailsWindow.Owner = this;
            detailsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            detailsWindow.ShowDialog();*/
        }

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadDataset(ViewModel.Store.DataDirectory);
        }

        private void showPetDetails(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedDataset = (Dataset) PetGrid.SelectedItem;

                /*if (selectedDataset.Kind == Dataset.DatasetKind.PetResult)
                {
                    var resultsDirectory = System.IO.Path.Combine(ViewModel.Store.DataDirectory,
                        DatasetStore.ResultsDirectory);

                    var detailsWindow = new PetDetailsControl(selectedDataset, resultsDirectory, ViewModel);
                    detailsWindow.Owner = this;
                    detailsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    detailsWindow.ShowDialog();
                }*/
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
            var taskDialog = new TaskDialog
            {
                WindowTitle = "Clean results folder",
                MainInstruction = "Are you sure you want to clean up the results folder?"
            };

            var cleanButton = new TaskDialogButton()
            {
                ButtonType = ButtonType.Custom,
                Text = "Clean results folder",
                CommandLinkNote =
                    "All folders in the results folder which are not included in the library will be moved to the deleted folder."
            };
            taskDialog.Buttons.Add(cleanButton);

            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel)
            {
                Default = true
            });
            taskDialog.CenterParent = true;
            taskDialog.ButtonStyle = TaskDialogButtonStyle.CommandLinks;
            var selectedButton = taskDialog.ShowDialog(this);

            

            if (selectedButton == cleanButton)
            {
                SetNativeEnabled(false);
                var progressDialog = new ProgressDialog
                {
                    Text = "Cleaning results folder...",
                    ProgressCurrentCount = 0,
                    ProgressTotalCount = 1,
                    Owner = this
                };
                progressDialog.Show();

                var store = ViewModel.Store;
                var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

                var cleanupTask = Task.Factory.StartNew(() =>
                {
                    store.Cleanup((completed, total) =>
                    {
                        Task.Factory.StartNew(() =>
                        {
                            progressDialog.ProgressCurrentCount = completed;
                            progressDialog.ProgressTotalCount = total;
                        }, new CancellationToken(), TaskCreationOptions.None, uiContext);
                    });
                }).ContinueWith(task =>
                {
                    SetNativeEnabled(true); //this must appear first, or we lose focus when the progress dialog closes
                    progressDialog.Close();
                }, uiContext);
            }
        }

        private void PetGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PetDetailsPanel.Children.Clear();

            //If we recreate this intermediate panel every time this method's called, we can
            //avoid displaying the wrong results if the user changes focus while data's loading
            var detailsPanel = new DockPanel();
            PetDetailsPanel.Children.Add(detailsPanel);

            var placeholderPanel = new PlaceholderDetailsPanel();
            detailsPanel.Children.Add(placeholderPanel);
            try
            {
                if (PetGrid.SelectedItem != null)
                {
                    var selectedDataset = (Dataset) PetGrid.SelectedItem;

                    if (selectedDataset.Kind == Dataset.DatasetKind.PetResult)
                    {
                        placeholderPanel.IsLoading = true;
                        var resultsDirectory = System.IO.Path.Combine(ViewModel.Store.DataDirectory,
                            DatasetStore.ResultsDirectory);

                        var loadTask = Task<PetDetailsViewModel>.Factory.StartNew(() =>
                        {
                            return new PetDetailsViewModel(selectedDataset, resultsDirectory);
                        });
                        
                        loadTask.ContinueWith(task =>
                        {
                            if (!task.IsCanceled)
                            {
                                if (task.Exception != null)
                                {
                                    placeholderPanel.IsLoading = false;
                                    placeholderPanel.DisplayText =
                                        "An error occurred while inspecting selected object: \n";

                                    foreach (var exception in task.Exception.InnerExceptions)
                                    {
                                        placeholderPanel.DisplayText += "\n" + exception.Message;
                                    }
                                }
                                else
                                {
                                    var detailsControl = new PetDetailsControl(task.Result, ViewModel);

                                    detailsPanel.Children.Clear();
                                    detailsPanel.Children.Add(detailsControl);
                                }
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else
                    {
                        placeholderPanel.DisplayText = "No archive inspector";
                    }
                }
            }
            catch (Exception ex)
            {
                placeholderPanel.IsLoading = false;
                placeholderPanel.DisplayText = "An error occurred while inspecting selected object.";
                ShowErrorDialog("Error", "An error occurred while loading dataset details.", ex.Message, ex.ToString());
            }
        }

        private void TestBenchGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TestBenchDetailsPanel.Children.Clear();

            //If we recreate this intermediate panel every time this method's called, we can
            //avoid displaying the wrong results if the user changes focus while data's loading
            var detailsPanel = new DockPanel();
            TestBenchDetailsPanel.Children.Add(detailsPanel);

            var placeholderPanel = new PlaceholderDetailsPanel();
            detailsPanel.Children.Add(placeholderPanel);
            try
            {
                if (TestBenchGrid.SelectedItem != null)
                {
                    var selectedDataset = (Dataset)TestBenchGrid.SelectedItem;

                    if (selectedDataset.Kind == Dataset.DatasetKind.TestBenchResult)
                    {
                        placeholderPanel.IsLoading = true;
                        var datasetPath = System.IO.Path.Combine(ViewModel.Store.DataDirectory, DatasetStore.ResultsDirectory,
                            selectedDataset.Folders[0]);

                        var loadTask = Task<TBManifestViewModel>.Factory.StartNew(() =>
                        {
                            return new TBManifestViewModel(datasetPath);
                        });

                        loadTask.ContinueWith(task =>
                        {
                            if (!task.IsCanceled)
                            {
                                if (task.Exception != null)
                                {
                                    placeholderPanel.IsLoading = false;
                                    placeholderPanel.DisplayText =
                                        "An error occurred while inspecting selected object: \n";

                                    foreach (var exception in task.Exception.InnerExceptions)
                                    {
                                        placeholderPanel.DisplayText += "\n" + exception.Message;
                                    }
                                }
                                else
                                {
                                    var detailsControl = new TestBenchDetailsControl(datasetPath, task.Result);

                                    detailsPanel.Children.Clear();
                                    detailsPanel.Children.Add(detailsControl);
                                }
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else
                    {
                        placeholderPanel.DisplayText = "No inspector available";
                    }
                }
            }
            catch (Exception ex)
            {
                placeholderPanel.IsLoading = false;
                placeholderPanel.DisplayText = "An error occurred while inspecting selected object.";
                ShowErrorDialog("Error", "An error occurred while loading dataset details.", ex.Message, ex.ToString());
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAllPets();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.DeselectAllPets();
        }

        private void reRunJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJob = (JobViewModel) JobGrid.SelectedItem;

            ViewModel.JobStore.ReRunJob(selectedJob.Job);
        }

        private void abortJob_Click(object sender, RoutedEventArgs e)
        {
            var selectedJob = (JobViewModel)JobGrid.SelectedItem;

            ViewModel.JobStore.AbortJob(selectedJob.Job);
        }

        private void showJobInExplorer(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedJob = (JobViewModel)JobGrid.SelectedItem;
                var jobDirectoryPath = selectedJob.Job.WorkingDirectory;

                //Explorer doesn't accept forward slashes in paths
                jobDirectoryPath = jobDirectoryPath.Replace("/", "\\");

                Process.Start("explorer.exe", jobDirectoryPath);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while opening in Explorer.", "", ex.ToString());
            }
        }

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            string datasetPath;
            if (ViewModel.Store != null)
            {
                datasetPath = ViewModel.Store.DataDirectory;
            }
            else
            {
                datasetPath = ".";
            }
            var newDatasetListWindow = new DatasetListWindow(ViewModel.JobStore, this.instanceManager, datasetPath);
            newDatasetListWindow.Show();
        }
    }

    public class DatasetListWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler TrackedJobsChanged;

        public List<Dataset> PetDatasetsList { get; set; }
        public ICollectionView PetDatasets { get; set; }

        public List<Dataset> TestBenchDatasetsList { get; set; }
        public ICollectionView TestBenchDatasets { get; set; }

        public DatasetStore Store { get; set; }
        public JobStore JobStore { get; set; }

        public ICollectionView JobsView { get; set; }

        public delegate void DatasetLoadFailedCallback(Exception exception);

        public bool AllowArchive
        {
            get { return PetDatasetsList.Exists(dataset => dataset.Selected); }
        }

        private bool _datasetLoaded;
        public bool DatasetLoaded
        {
            get { return _datasetLoaded; }

            set
            {
                PropertyChanged.ChangeAndNotify(ref _datasetLoaded, value, () => DatasetLoaded);
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }

            set { PropertyChanged.ChangeAndNotify(ref _isLoading, value, () => IsLoading); }
        }

        private int _loadProgressCount;

        public int LoadProgressCount
        {
            get { return _loadProgressCount; }

            set { PropertyChanged.ChangeAndNotify(ref _loadProgressCount, value, () => LoadProgressCount); }
        }

        private int _loadTotalCount;

        public int LoadTotalCount
        {
            get { return _loadTotalCount; }

            set { PropertyChanged.ChangeAndNotify(ref _loadTotalCount, value, () => LoadTotalCount); }
        }

        private string _projectPath;

        public string ProjectPath
        {
            get { return _projectPath; }

            set { PropertyChanged.ChangeAndNotify(ref _projectPath, value, () => ProjectPath); }
        }

        public int ManagerPort
        {
            get { return JobStore.Port; }
        }

        public DatasetListWindowViewModel(JobStore jobStore)
        {
            Store = null;
            DatasetLoaded = false;
            IsLoading = false;
            LoadProgressCount = 0;
            LoadTotalCount = 1;
            ProjectPath = "No project loaded";
            PetDatasetsList = new List<Dataset>();
            PetDatasets = new ListCollectionView(PetDatasetsList);
            PetDatasets.SortDescriptions.Add(new SortDescription("Time", ListSortDirection.Descending));

            TestBenchDatasetsList = new List<Dataset>();
            TestBenchDatasets = new ListCollectionView(TestBenchDatasetsList);
            TestBenchDatasets.SortDescriptions.Add(new SortDescription("Time", ListSortDirection.Descending));

            if (jobStore == null)
            {
                JobStore = new JobStore();
            }
            else
            {
                JobStore = jobStore;
            }
            JobsView = new ListCollectionView(JobStore.TrackedJobs);
            JobStore.TrackedJobsChanged += (sender, args) =>
            {
                JobsView.Refresh();
                if (TrackedJobsChanged != null)
                {
                    TrackedJobsChanged(this, EventArgs.Empty);
                }
            };
            JobStore.JobCompleted += (sender, args) =>
            {
                if (DatasetLoaded)
                {
                    LoadDataset(Store.DataDirectory, exception => Console.WriteLine(exception));
                }
            };
        }

        public void LoadDataset(string path, DatasetLoadFailedCallback callback)
        {
            Store = null;
            DatasetLoaded = false;
            IsLoading = true;
            LoadProgressCount = 0;
            LoadTotalCount = 1;
            ProjectPath = "No project loaded";
            PetDatasetsList.Clear();
            PetDatasets.Refresh();
            TestBenchDatasetsList.Clear();
            TestBenchDatasets.Refresh();

            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            Task<DatasetStore> loadTask = new Task<DatasetStore>(() =>
            {
                var newStore = new DatasetStore(path, (completed, total) =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        LoadProgressCount = completed;
                        LoadTotalCount = total;
                    }, new CancellationToken(), TaskCreationOptions.None, uiContext);
                });

                return newStore;
            });

            loadTask.ContinueWith(task =>
            {
                IsLoading = false;
                if (task.Exception != null)
                {
                    if (callback != null)
                    {
                        task.Exception.Handle(exception =>
                        {
                            if (callback != null)
                            {
                                callback(exception);
                            }
                            return true;
                        });
                    }
                }
                else
                {
                    Store = task.Result;

                    PetDatasetsList.Clear();
                    PetDatasetsList.AddRange(Store.ResultDatasets);
                    PetDatasetsList.AddRange(Store.ArchiveDatasets);
                    PetDatasets.Refresh();

                    TestBenchDatasetsList.Clear();
                    TestBenchDatasetsList.AddRange(Store.TestbenchDatasets);
                    TestBenchDatasets.Refresh();

                    ProjectPath = System.IO.Path.GetFullPath(path);
                    DatasetLoaded = true;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()); //this should run on the UI thread

            loadTask.Start();
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
            LoadDataset(Store.DataDirectory, null);
        }

        public void SelectAllPets()
        {
            foreach (var dataset in Store.ResultDatasets)
            {
                dataset.Selected = true;
            }

            foreach (var dataset in Store.ArchiveDatasets)
            {
                dataset.Selected = true;
            }
        }

        public void DeselectAllPets()
        {
            foreach (var dataset in Store.ResultDatasets)
            {
                dataset.Selected = false;
            }

            foreach (var dataset in Store.ArchiveDatasets)
            {
                dataset.Selected = false;
            }
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, CompareOptions.IgnoreCase) >= 0;
        }
    }
}
