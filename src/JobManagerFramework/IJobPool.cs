using System.Deployment.Internal;
using JobManager;

namespace JobManagerFramework
{
    public interface IJobPool
    {
        /// <summary>
        /// Kills all currently running/pending tasks.
        /// Releases all resources.
        /// </summary>
        void Dispose();

        void EnqueueJob(Job j);
        bool AbortJob(Job j);

        int GetNumberOfUnfinishedJobs();
    }
}