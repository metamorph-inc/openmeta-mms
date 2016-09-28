using System.ComponentModel;
using System.Security;
using JobManager;
using JobManagerFramework;

namespace PETBrowser
{
    public class JobViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Job Job { get; private set; }

        public string Title { get { return Job.Title; } }
        public string TestBenchName { get { return Job.TestBenchName; } }
        public string WorkingDirectory { get { return Job.WorkingDirectory; } }
        public string RunCommand { get { return Job.RunCommand; } }
        public Job.StatusEnum Status { get { return Job.Status; } }

        public bool AllowReRun
        {
            get { return Job.IsFailed() || Job.Status == Job.StatusEnum.Succeeded; }
        }

        public bool AllowAbort { get { return !AllowReRun; } }

        public JobViewModel(Job job)
        {
            Job = job;
            ((JobImpl) job).JobStatusChanged += JobStatusChanged;
        }

        private void JobStatusChanged(JobImpl job, Job.StatusEnum status)
        {
            // Notify that Status and all dependent properties have changed
            PropertyChanged.Notify(() => Status);
            PropertyChanged.Notify(() => AllowReRun);
            PropertyChanged.Notify(() => AllowAbort);
        }
    }
}