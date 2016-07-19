using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public ProgressDialog()
        {
            InitializeComponent();
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { PropertyChanged.ChangeAndNotify(ref _text, value, () => Text); }
        }

        private int _progressCurrentCount;
        public int ProgressCurrentCount
        {
            get { return _progressCurrentCount; }
            set { PropertyChanged.ChangeAndNotify(ref _progressCurrentCount, value, () => ProgressCurrentCount); }
        }

        private int _progressTotalCount;
        public int ProgressTotalCount
        {
            get { return _progressTotalCount; }
            set { PropertyChanged.ChangeAndNotify(ref _progressTotalCount, value, () => ProgressTotalCount); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
