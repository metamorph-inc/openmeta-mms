using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public TestBenchDetailsWindow(string manifestPath)
        {
            this.ViewModel = new TBManifestViewModel(manifestPath);
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
