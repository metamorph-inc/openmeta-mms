using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using JobManager;
using JobManagerFramework;

namespace PETBrowser
{
    public class JobViewModel : INotifyPropertyChanged
    {
        private const int MaxProgressMessageHistory = 1000;

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

        public Queue<String> ProgressMessageHistoryQueue { get; private set; }

        public string ProgressMessageHistory
        {
            get
            {
                return string.Join("\n", ProgressMessageHistoryQueue);
            }
        }

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
            ProgressMessageHistoryQueue = new Queue<string>();
        }

        private void OnJobProgressChanged(JobImpl job, string progressMessage, int progressCurrent, int progressTotal)
        {
            // OnJobProgressChanged typically isn't called from the UI thread--  so we need to
            // make sure that we update progress on the main thread
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (ProgressMessageHistoryQueue.Count >= MaxProgressMessageHistory)
                {
                    ProgressMessageHistoryQueue.Dequeue();
                }

                if (!ProgressIsIndeterminate)
                {
                    var newMessage = $"({ProgressCurrent}/{ProgressTotal}) {ProgressMessage}";
                    ProgressMessageHistoryQueue.Enqueue(newMessage);
                }
                else
                {
                    var newMessage = $"({ProgressCurrent}) {ProgressMessage}";
                    ProgressMessageHistoryQueue.Enqueue(newMessage);
                }


                PropertyChanged.Notify(() => ProgressMessage);
                PropertyChanged.Notify(() => ProgressCurrent);
                PropertyChanged.Notify(() => ProgressTotal);
                PropertyChanged.Notify(() => ProgressIsIndeterminate);
                PropertyChanged.Notify(() => ProgressMessageHistory);
            }));
        }

        private void JobStatusChanged(JobImpl job, Job.StatusEnum status)
        {
            // Not guaranteed to be called on the main thread--  so make sure we update
            // on the main thread only
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                // Notify that Status and all dependent properties have changed
                PropertyChanged.Notify(() => Status);
                PropertyChanged.Notify(() => AllowReRun);
                PropertyChanged.Notify(() => AllowAbort);

                PropertyChanged.Notify(() => ProgressMessage);
                PropertyChanged.Notify(() => ProgressCurrent);
                PropertyChanged.Notify(() => ProgressTotal);
                PropertyChanged.Notify(() => ProgressIsIndeterminate);
            }));
        }
    }
}