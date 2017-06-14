﻿using System;
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

            Unloaded += (sender, args) =>
            {
                ViewModel.Dispose();
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

            VisualizerLauncher.LaunchVisualizer(selectedConfig.ConfigPath);
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
    }
}
