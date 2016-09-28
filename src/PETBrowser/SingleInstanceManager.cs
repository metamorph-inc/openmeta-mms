using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using JobManagerFramework;

namespace PETBrowser
{
    public class SingleInstanceManager : IDisposable
    {
        public const int PortNumber = 36010;
        public const string ServerName = "BrowserInstance";

        private InstanceImpl Instance { get; set; }
        private TcpServerChannel ServerChannel { get; set; }

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
            BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            System.Collections.IDictionary clientTcpChannelProperties = new System.Collections.Hashtable();
            clientTcpChannelProperties["name"] = "ClientTcpChan2";
            ChannelServices.RegisterChannel(
                new System.Runtime.Remoting.Channels.Tcp.TcpClientChannel(clientTcpChannelProperties,
                    new BinaryClientFormatterSinkProvider()), false);

            System.Collections.IDictionary TcpChannelProperties = new Dictionary<string, object>();
            TcpChannelProperties["port"] = PortNumber;
            TcpChannelProperties["bindTo"] = System.Net.IPAddress.Loopback.ToString();
            TcpChannelProperties["name"] = "instance";
            ServerChannel = new System.Runtime.Remoting.Channels.Tcp.TcpServerChannel(TcpChannelProperties, serverProv);
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
                var instanceConnection = new Uri("tcp://" + System.Net.IPAddress.Loopback.ToString() + ":" + SingleInstanceManager.PortNumber + "/" + SingleInstanceManager.ServerName);
                var instance = (Instance)Activator.GetObject(typeof(Instance), instanceConnection.OriginalString);
                instance.CreateBrowserForWorkingDirectory(workingDirectory);
                return true;
            }
            catch (System.Net.Sockets.SocketException)
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
