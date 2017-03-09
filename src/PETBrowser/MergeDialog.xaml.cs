using System;
using System.Collections.Generic;
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

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for MergeDialog.xaml
    /// </summary>
    public partial class MergeDialog : Window
    {
        private IEnumerable<Dataset> DatasetsToMerge { get; set; }

        private string DataDirectoryPath { get; set; }

        public MergeDialog(IEnumerable<Dataset> datasetsToMerge, string dataDirectoryPath)
        {
            InitializeComponent();

            DatasetsToMerge = datasetsToMerge;
            DataDirectoryPath = dataDirectoryPath;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            PetMerger.MergePets(this.MergedPetNameTextBox.Text, DatasetsToMerge, DataDirectoryPath);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
