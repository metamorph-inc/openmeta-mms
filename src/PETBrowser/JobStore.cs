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
    public class JobStore
    {
        private JobManagerFramework.JobManager Manager { get; set; }
        public List<JobViewModel> TrackedJobs { get; private set; }

        public event EventHandler TrackedJobsChanged;

        private TaskScheduler UiTaskScheduler { get; set; }

        //Constructor MUST be called on UI thread (we capture the UI synchronization context for use later)
        public JobStore()
        {

            UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            TrackedJobs = new List<JobViewModel>();
            Manager = new JobManagerFramework.JobManager(35010,
                new JobManagerFramework.JobManager.JobManagerConfiguration());

            Manager.JobAdded += Manager_JobAdded;
        }

        public void ReRunJob(Job j)
        {
            Manager.ReRunJobs(new [] {j});
        }

        private void Manager_JobAdded(object sender, JobManagerFramework.JobManager.JobAddedEventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                TrackedJobs.Add(new JobViewModel(e.Job));
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