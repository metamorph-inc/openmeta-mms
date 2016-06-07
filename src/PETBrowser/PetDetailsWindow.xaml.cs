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
    /// Interaction logic for PetDetailsWindow.xaml
    /// </summary>
    public partial class PetDetailsWindow : Window
    {
        public PetDetailsViewModel ViewModel
        {
            get { return (PetDetailsViewModel)DataContext; }
            set { DataContext = value; }
        }

        public PetDetailsWindow(Dataset dataset)
        {
            this.ViewModel = new PetDetailsViewModel(dataset);
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
