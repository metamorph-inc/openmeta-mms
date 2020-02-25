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
        public string ProgressMessage
        {
            get { return ((JobImpl) Job).ProgressMessage; }
        }

        public int ProgressCurrent
        {
            get
            {
                if (!ProgressIsIndeterminate)
                {
                    return ((JobImpl)Job).ProgressCurrent;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int ProgressTotal
        {
            get
            {
                if (!ProgressIsIndeterminate)
                {
                    return ((JobImpl)Job).ProgressTotal;
                }
                else
                {
                    return 100;
                }
            }
        }

        public bool ProgressIsIndeterminate
        {
            get
            {
                    return ((JobImpl)Job).ProgressTotal <= 0;
            }
        }

        public string ProgressMessageHistory { get; private set; }

        public bool AllowReRun
        {
            get { return Job.IsFailed() || Job.Status == Job.StatusEnum.Succeeded; }
        }

        public bool AllowAbort { get { return !AllowReRun; } }

        public JobViewModel(Job job)
        {
            Job = job;
            ((JobImpl) job).JobStatusChanged += JobStatusChanged;
            ((JobImpl) job).JobProgressChanged += OnJobProgressChanged;
        }

        private void OnJobProgressChanged(JobImpl job, string progressMessage, int progressCurrent, int progressTotal)
        {
            if (!string.IsNullOrEmpty(ProgressMessageHistory))
            {
                ProgressMessageHistory += "\n";
            }

            if (!ProgressIsIndeterminate)
            {
                ProgressMessageHistory += $"({ProgressCurrent}/{ProgressTotal}) {ProgressMessage}";
            }
            else
            {
                ProgressMessageHistory += $"({ProgressCurrent}) {ProgressMessage}";
            }
            

            PropertyChanged.Notify(() => ProgressMessage);
            PropertyChanged.Notify(() => ProgressCurrent);
            PropertyChanged.Notify(() => ProgressTotal);
            PropertyChanged.Notify(() => ProgressIsIndeterminate);
            PropertyChanged.Notify(() => ProgressMessageHistory);
        }

        private void JobStatusChanged(JobImpl job, Job.StatusEnum status)
        {
            // Notify that Status and all dependent properties have changed
            PropertyChanged.Notify(() => Status);
            PropertyChanged.Notify(() => AllowReRun);
            PropertyChanged.Notify(() => AllowAbort);

            PropertyChanged.Notify(() => ProgressMessage);
            PropertyChanged.Notify(() => ProgressCurrent);
            PropertyChanged.Notify(() => ProgressTotal);
            PropertyChanged.Notify(() => ProgressIsIndeterminate);
        }
    }
}