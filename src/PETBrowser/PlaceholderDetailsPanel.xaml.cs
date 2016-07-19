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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for PlaceholderDetailsPanel.xaml
    /// </summary>
    public partial class PlaceholderDetailsPanel : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _displayText;

        public string DisplayText
        {
            get { return _displayText; }
            set { PropertyChanged.ChangeAndNotify(ref _displayText, value, () => DisplayText); }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { PropertyChanged.ChangeAndNotify(ref _isLoading, value, () => IsLoading); }
        }

        public PlaceholderDetailsPanel()
        {
            InitializeComponent();
            IsLoading = false;
            DisplayText = "No inspectable item selected";
        }
    }
}
