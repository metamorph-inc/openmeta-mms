using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Principal;
using System.Threading;

namespace PETBrowser
{
    public class SingleInstanceManager : IDisposable
    {
        public const string PortNameBase = "MetaPETBrowser";
        public const string ServerName = "BrowserInstance";

        public static string PortName
        {
            get { return PortNameBase + WindowsIdentity.GetCurrent().User; }
        }

        private InstanceImpl Instance { get; set; }
        private IpcServerChannel ServerChannel { get; set; }

        public class OnCreateForWorkingDirectoryEventArgs : EventArgs
        {
            public string WorkingDirectory { get; set; }

            public OnCreateForWorkingDirectoryEventArgs(string workingDirectory) : base()
            {
                WorkingDirectory = workingDirectory;
            }
        }

        public event EventHandler<OnCreateForWorkingDirectoryEventArgs> OnCreateForWorkingDirectory;

        public SingleInstanceManager()
        {
            var provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            ServerChannel = new IpcServerChannel(ServerName, PortName, provider);
            ChannelServices.RegisterChannel(ServerChannel, false);

            Instance = new InstanceImpl();

            Instance.OnCreateForWorkingDirectory += (sender, args) =>
            {
                if (OnCreateForWorkingDirectory != null)
                {
                    OnCreateForWorkingDirectory(this, new OnCreateForWorkingDirectoryEventArgs(args.WorkingDirectory));
                }
            };

            RemotingServices.Marshal(Instance, ServerName);
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                if (ChannelServices.RegisteredChannels.Contains(ServerChannel))
                {
                    ChannelServices.UnregisterChannel(ServerChannel);
                }
            }
        }
    }

    public class SingleInstanceClient
    {
        public static bool OpenBrowserIfRunning(string workingDirectory)
        {
            try
            {
                var instanceConnection = new Uri("ipc://" + SingleInstanceManager.PortName + "/" + SingleInstanceManager.ServerName);
                var instance = (Instance)Activator.GetObject(typeof(Instance), instanceConnection.OriginalString);

                instance.CreateBrowserForWorkingDirectory(workingDirectory);
                return true;
            }
            catch (RemotingException)
            {
                return false;
            }
        }
    }

    public abstract class Instance : MarshalByRefObject
    {
        public abstract void CreateBrowserForWorkingDirectory(string workingDirectory);
    }

    public class InstanceImpl : Instance
    {
        public class OnCreateForWorkingDirectoryEventArgs : EventArgs
        {
            public string WorkingDirectory { get; set; }

            public OnCreateForWorkingDirectoryEventArgs(string workingDirectory) : base()
            {
                WorkingDirectory = workingDirectory;
            }
        }

        public event EventHandler<OnCreateForWorkingDirectoryEventArgs> OnCreateForWorkingDirectory;

        public override void CreateBrowserForWorkingDirectory(string workingDirectory)
        {
            if (OnCreateForWorkingDirectory != null)
            {
                OnCreateForWorkingDirectory(this, new OnCreateForWorkingDirectoryEventArgs(workingDirectory));
            }
        }

        /**
         * Required to prevent remote object from being destroyed after timeout (probably five minutes)
         */
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
