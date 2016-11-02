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
    /// Interaction logic for VisualizerDialog.xaml
    /// </summary>
    public partial class VisualizerDialog : Window
    {
        public VisualizerDialog()
        {
            InitializeComponent();
            this.OkButton.Focus();
        }

        public bool CfgID
        {
            get { return (bool)this.CfgIDCheckBox.IsChecked; }
            set { this.CfgIDCheckBox.IsChecked = true; }
        }

        public bool Alternatives
        {
            get { return (bool)this.AlternativesCheckBox.IsChecked; }
            set { this.AlternativesCheckBox.IsChecked = true; }
        }

        public bool Optionals
        {
            get { return (bool)this.OptionalsCheckBox.IsChecked; }
            set { this.OptionalsCheckBox.IsChecked = true; }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
