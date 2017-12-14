using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JobManagerFramework.RemoteExecution;
using Ookii.Dialogs.Wpf;

namespace PETBrowser
{
    /// <summary>
    /// Interaction logic for RemoteServerPromptDialog.xaml
    /// </summary>
    public partial class RemoteServerPromptDialog : Window
    {
        public RemoteServerPromptDialogViewModel ViewModel
        {
            get { return (RemoteServerPromptDialogViewModel)DataContext; }
            set { DataContext = value; }
        }

        public string Password
        {
            get { return PasswordField.Password; }
        }

        public RemoteServerPromptDialog()
        {
            this.ViewModel = new RemoteServerPromptDialogViewModel();
            ViewModel.ServerVerified += (sender, args) => this.DialogResult = true;

            ViewModel.ServerVerificationFailed += (sender, args) =>
            {
                if (args.SourceException is RemoteExecutionService.RequestFailedException &&
                    ((RemoteExecutionService.RequestFailedException) args.SourceException).StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    ShowErrorDialog("Server verification failed",
                        "The username or password is incorrect.",
                        "Verify that your username and password are entered correctly and that you're connecting to the correct server, then try again.",
                        args.SourceException.ToString());
                }
                else
                {
                    ShowErrorDialog("Server verification failed",
                        "The remote server couldn't be verified.",
                        "Verify that you are using the correct server address, username, and password and try again.",
                        args.SourceException.ToString());
                }
            };

            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.VerifyServer(PasswordField.Password);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
    }

    public class RemoteServerPromptDialogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<EventArgs> ServerVerified;

        public event EventHandler<ServerVerificationFailedEventArgs> ServerVerificationFailed; 

        public class ServerVerificationFailedEventArgs : EventArgs
        {
            public Exception SourceException { get; set; }

            public ServerVerificationFailedEventArgs(Exception sourceException)
            {
                SourceException = sourceException;
            }
        }

        public RemoteServerPromptDialogViewModel()
        {
            _serverName = "";
            _username = "";
            _verifying = false;
        }

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }

            set
            {
                PropertyChanged.ChangeAndNotify(ref _serverName, value, () => ServerName);
                PropertyChanged.Notify(() => OkButtonEnabled);
            }
        }

        private string _username;

        public string Username
        {
            get { return _username; }

            set
            {
                PropertyChanged.ChangeAndNotify(ref _username, value, () => Username);
                PropertyChanged.Notify(() => OkButtonEnabled);
            }
        }

        public bool OkButtonEnabled
        {
            get { return !Verifying && ServerName.Length > 0 && Username.Length > 0; }
        }

        public bool _verifying;

        public bool Verifying
        {
            get { return _verifying; }

            set
            {
                PropertyChanged.ChangeAndNotify(ref _verifying, value, () => Verifying);
                PropertyChanged.Notify(() => OkButtonEnabled);
            }
        }

        public void VerifyServer(string password)
        {
            Verifying = true;
            var verifyServerTask = new Task<Exception>(() =>
            {
                var remoteService = new RemoteExecutionService(ServerName, Username, password);

                try
                {
                    remoteService.PingServer(); //TODO: validate that we received the response we expected
                    return null;
                }
                catch (Exception e)
                {
                    return e;
                }
            });

            verifyServerTask.ContinueWith(task =>
            {
                if (task.Result == null)
                {
                    // Verification completed successfully
                    if (ServerVerified != null)
                    {
                        ServerVerified(this, EventArgs.Empty);
                    }
                }
                else
                {
                    Verifying = false;

                    if (ServerVerificationFailed != null)
                    {
                        ServerVerificationFailed(this, new ServerVerificationFailedEventArgs(task.Result));
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            verifyServerTask.Start();
        }
    }
}
