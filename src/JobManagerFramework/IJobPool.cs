using System;
using System.Deployment.Internal;
using JobManager;

namespace JobManagerFramework
{
    public interface IJobPool : IDisposable
    {
        void EnqueueJob(Job j);
        bool AbortJob(Job j);

        int GetNumberOfUnfinishedJobs();
    }
}