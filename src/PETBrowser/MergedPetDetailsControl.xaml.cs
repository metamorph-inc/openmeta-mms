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
    /// Interaction logic for PetDetailsControl.xaml
    /// </summary>
    public partial class MergedPetDetailsControl : UserControl
    {
        public MergedPetDetailsViewModel ViewModel
        {
            get { return (MergedPetDetailsViewModel)DataContext; }
            set { DataContext = value; }
        }

        private DatasetListWindowViewModel DatasetViewModel { get; set; }

        public MergedPetDetailsControl(MergedPetDetailsViewModel mergedPetDetailsViewModel, DatasetListWindowViewModel datasetViewModel)
        {
            this.ViewModel = mergedPetDetailsViewModel;
            this.DatasetViewModel = datasetViewModel;
            InitializeComponent();

            /*
             * WPF doesn't have an event that triggers when a Control is removed permanently from the tree, and Load/Unload can be
             * called multiple times if an object is hidden/reshown (i.e. if the user switches tabs)--  so, to properly make sure
             * we clean up our VisualizerExitedEvent handler, we need to do it on Unload, and re-register on Load (and check for
             * any that we might've missed while offscreen).
             */
            Loaded += (sender, args) =>
            {
                ViewModel.RegisterForVisualizerExitedEvents();
                ViewModel.UpdateVisualizerSessionStatus();
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.UnregisterForVisualizerExitedEvents();
            };
        }



        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            //this.Close();
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
            taskDialog.ShowDialog(Window.GetWindow(this));
            //MessageBox.Show(this, "The selected folder doesn't appear to be a valid data folder.",
            //    "Error loading datasets", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MgaFilename_Clicked(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var mgaPath = this.ViewModel.MgaFilePath;

                //Explorer doesn't accept forward slashes in paths
                mgaPath = mgaPath.Replace("/", "\\");

                Process.Start("explorer.exe", "/select,\"" + mgaPath + "\"");
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while opening in Explorer.", "", ex.ToString());
            }
        }

        private void LaunchVisualizerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedConfig = (MergedPetDetailsViewModel.VisualizerSession) VisualizerSessionsGrid.SelectedItem;
            selectedConfig.VisualizerNotRunning = false;

            VisualizerLauncher.LaunchAnalysisTool(ViewModel.AnalysisTools.DefaultAnalysisTool, selectedConfig.ConfigPath, ViewModel.DetailsDataset, DatasetViewModel.Store.DataDirectory);
        }

        

        private void NewSessionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sessionNameDialog = new PromptDialog { Owner = Window.GetWindow(this), Text = "" };

                bool? result = sessionNameDialog.ShowDialog();

                if (result == true && !string.IsNullOrEmpty(sessionNameDialog.Text))
                {
                    Console.WriteLine("New Session: {0}", sessionNameDialog.Text);
                    ViewModel.CreateNewVisualizerSession(sessionNameDialog.Text);
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Session creation error", "An error occurred while creating a new Visualizer session.", ex.Message, ex.ToString());
            }
        }

        private bool isContextMenuOpen = false;
        private void AnalysisToolsButton_Click(object sender, RoutedEventArgs e)
        {
            var source = sender as Button;

            if (source != null && source.ContextMenu != null)
            {
                // Only open the ContextMenu when it is not already open. If it is already open,
                // when the button is pressed the ContextMenu will lose focus and automatically close.
                if (!isContextMenuOpen)
                {
                    source.ContextMenu.AddHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(ContextMenu_Closed), true);
                    // If there is a drop-down assigned to this button, then position and display it 
                    // Note: normally we'd position the context menu below the source button, but since
                    // we're simulating a split button, we're placing it underneath the "main" launch button
                    source.ContextMenu.PlacementTarget = LaunchVisualizerButton;
                    //source.ContextMenu.Placement = PlacementMode.Bottom;
                    source.ContextMenu.IsOpen = true;
                    isContextMenuOpen = true;
                }
            }
        }

        void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            isContextMenuOpen = false;
            var contextMenu = sender as ContextMenu;
            if (contextMenu != null)
            {
                contextMenu.RemoveHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(ContextMenu_Closed));
            }
        }

        private void PetAnalysisToolRun(object sender, RoutedEventArgs e)
        {
            var source = (MenuItem) sender;

            var analysisTool = (AnalysisTool) source.DataContext;
            Console.WriteLine("Running analysis tool {0}", analysisTool.DisplayName);

            var selectedConfig = (MergedPetDetailsViewModel.VisualizerSession)VisualizerSessionsGrid.SelectedItem;
            selectedConfig.VisualizerNotRunning = false;

            VisualizerLauncher.LaunchAnalysisTool(analysisTool, selectedConfig.ConfigPath, ViewModel.DetailsDataset, DatasetViewModel.Store.DataDirectory);
        }

        private void showFolderInExplorer(object sender, RoutedEventArgs e)
        {
            var row = sender as FrameworkElement;
            if (row != null)
            {
                var path = row.DataContext as string;
                OpenFolderInExplorer(path);
            }
        }

        private void showAllFoldersInExplorer(object sender, RoutedEventArgs e)
        {
            this.ViewModel.DetailsDataset.SourceFolders.ForEach(OpenFolderInExplorer);
        }

        private void OpenFolderInExplorer(string folderPath)
        {
            try
            {
                Process.Start("explorer.exe", folderPath);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Error", "An error occurred while opening in Explorer.", "", ex.ToString());
            }
        }
    }
}
