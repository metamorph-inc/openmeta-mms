using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace JobManagerFramework.ProgressFeedback
{
    class ProgressFeedbackServerManager : IDisposable
    {
        private Server GrpcServer { get; set; }
        public bool ServerStarted { get; private set; }

        public int ServerBoundPort => GrpcServer.Ports.First().BoundPort;

        public string ServerAddress => $"localhost:{ServerBoundPort}";

        public ProgressFeedbackServerManager(ProgressFeedbackService.UpdateJobProgressHandler updateProgressDelegate)
        {
            var progressFeedbackService = new ProgressFeedbackService();
            progressFeedbackService.UpdateJobProgress += updateProgressDelegate;

            GrpcServer = new Server
            {
                Services = {Gen.ProgressFeedback.BindService(progressFeedbackService)},
                Ports = {new ServerPort("localhost", ServerPort.PickUnused, ServerCredentials.Insecure)}
            };

            ServerStarted = false;
        }

        public void Start()
        {
            ServerStarted = true;
            GrpcServer.Start();
        }

        public void Stop()
        {
            GrpcServer.ShutdownAsync().Wait();
            ServerStarted = false;
        }

        public void Dispose()
        {
            if (ServerStarted)
            {
                Stop();
            }
        }
    }
}
