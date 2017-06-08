using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobManager;
using JobManagerFramework;


namespace PETBrowser
{
    public class JobStore : IDisposable, INotifyPropertyChanged
    {
        private const int USER_THREAD_COUNT_MAX = 8;

        public event PropertyChangedEventHandler PropertyChanged;

        private JobManagerFramework.JobManager Manager { get; set; }
        public List<JobViewModel> TrackedJobs { get; private set; }

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

        public event EventHandler<JobManagerFramework.JobManager.JobCollectionAddedEventArgs> JobCollectionAdded;

        private TaskScheduler UiTaskScheduler { get; set; }

        public int UnfinishedJobCount
        {
            get { return Manager.UnfinishedJobCount; }
        }

        public bool HasIncompleteSots
        {
            get { return Manager.HasIncompleteSots; }
        }

        private int _selectedThreadCount;

        public int SelectedThreadCount
        {
            get { return _selectedThreadCount; }

            set
            {
                PropertyChanged.ChangeAndNotify(ref _selectedThreadCount, value, () => SelectedThreadCount);

                if (Manager != null)
                {
                    Manager.LocalConcurrentThreads = value;
                }

                Properties.Settings.Default.SelectedThreadCount = value;
                Properties.Settings.Default.Save();
            }
        }

        private IList<int> _threadOptionsList;

        public IList<int> ThreadOptionsList
        {
            get { return _threadOptionsList; }

            set { PropertyChanged.ChangeAndNotify(ref _threadOptionsList, value, () => ThreadOptionsList); }
        }

        private int _physicalCoreCount;

        public int PhysicalCoreCount
        {
            get { return _physicalCoreCount; }

            set { PropertyChanged.ChangeAndNotify(ref _physicalCoreCount, value, () => PhysicalCoreCount); }
        }

        public bool HasNoRunningJobs
        {
            get { return TrackedJobs.All(model => model.AllowAbort != true); }
        }

        //Constructor MUST be called on UI thread (we capture the UI synchronization context for use later)
        public JobStore()
        {
            PhysicalCoreCount = LocalPool.GetNumberOfPhysicalCores();
            SelectedThreadCount = Properties.Settings.Default.SelectedThreadCount; //TODO: load setting from a previous session?
            if (SelectedThreadCount == 0)
            {
                SelectedThreadCount = PhysicalCoreCount;
            }
            ThreadOptionsList = Enumerable.Range(1, USER_THREAD_COUNT_MAX).ToList();
            if (PhysicalCoreCount > USER_THREAD_COUNT_MAX)
            {
                ThreadOptionsList.Add(PhysicalCoreCount);
            }

            UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            TrackedJobs = new List<JobViewModel>();
            Manager = new JobManagerFramework.JobManager(SelectedThreadCount);
            Manager.Server.IsRemote = false;
            Manager.LoadSavedJobs();

            Manager.JobAdded += Manager_JobAdded;
            Manager.JobCollectionAdded += JobCollectionAddedHandler;

            this.TrackedJobsChanged += (sender, args) =>
            {
                PropertyChanged.Notify(() => HasNoRunningJobs);
            };
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

                        //Regardless of status, always notify that HasNoRunningJobs might've changed when a job's status changes
                        PropertyChanged.Notify(() => HasNoRunningJobs);
                    });
                    
                };
                InvokeTrackedJobsChanged();
            });
        }

        private void JobCollectionAddedHandler(object o, JobManagerFramework.JobManager.JobCollectionAddedEventArgs jobCollectionAddedEventArgs)
        {
            InvokeOnMainThread(() =>
            {
                PetMerger.BuildPetDirectory(jobCollectionAddedEventArgs.JobCollection);

                if (JobCollectionAdded != null)
                {
                    JobCollectionAdded(this, jobCollectionAddedEventArgs);
                }
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