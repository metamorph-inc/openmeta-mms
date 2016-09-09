using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobManager;
using JobManagerFramework;


namespace PETBrowser
{
    public class JobStore : IDisposable
    {
        private JobManagerFramework.JobManager Manager { get; set; }
        public List<JobViewModel> TrackedJobs { get; private set; }

        public int Port
        {
            get { return Manager.Port; }
        }

        public event EventHandler TrackedJobsChanged;

        public class JobCompletedEventArgs : EventArgs
        {
            public Job Job { get; set; }

            public JobCompletedEventArgs(Job job)
            {
                Job = job;
            }
        }

        public event EventHandler<JobCompletedEventArgs> JobCompleted;

        private TaskScheduler UiTaskScheduler { get; set; }

        public int UnfinishedJobCount
        {
            get { return Manager.UnfinishedJobCount; }
        }

        public bool HasIncompleteSots
        {
            get { return Manager.HasIncompleteSots; }
        }

        //Constructor MUST be called on UI thread (we capture the UI synchronization context for use later)
        public JobStore()
        {

            UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            TrackedJobs = new List<JobViewModel>();
            Manager = new JobManagerFramework.JobManager(35010,
                new JobManagerFramework.JobManager.JobManagerConfiguration());

            Manager.JobAdded += Manager_JobAdded;
        }

        private bool disposed = false;

        public void Dispose()
        {
            Manager.Dispose();
        }

        public void ReRunJob(Job j)
        {
            Manager.ReRunJobs(new [] {j});
        }

        public void AbortJob(Job j)
        {
            Manager.AbortJobs(new [] {j});
        }

        private void Manager_JobAdded(object sender, JobManagerFramework.JobManager.JobAddedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                TrackedJobs.Add(new JobViewModel(e.Job));
                ((JobImpl) e.Job).JobStatusChanged += (job, status) =>
                {
                    InvokeOnMainThread(() =>
                    {
                        if (job.Status == Job.StatusEnum.Succeeded || job.IsFailed())
                        {
                            if (JobCompleted != null)
                            {
                                JobCompleted(this, new JobCompletedEventArgs(e.Job));
                            }
                        }
                    });
                    
                };
                InvokeTrackedJobsChanged();
            });
        }

        private void JobStatusChanged(JobImpl job, Job.StatusEnum status)
        {
           InvokeOnMainThread(InvokeTrackedJobsChanged);
        }

        private void InvokeTrackedJobsChanged()
        {
            if (TrackedJobsChanged != null)
            {
                TrackedJobsChanged(this, EventArgs.Empty);
            }
        }

        private void InvokeOnMainThread(Action action)
        {
            Task.Factory.StartNew(action,
                CancellationToken.None,
                TaskCreationOptions.None,
                UiTaskScheduler);
        }
    }
}