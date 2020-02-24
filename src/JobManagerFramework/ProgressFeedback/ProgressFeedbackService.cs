using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using JobManagerFramework.ProgressFeedback.Gen;

namespace JobManagerFramework.ProgressFeedback
{
    class ProgressFeedbackService : Gen.ProgressFeedback.ProgressFeedbackBase
    {
        public delegate void UpdateJobProgressHandler(string jobId, string message, int currentProgress,
            int totalProgress);

        public event UpdateJobProgressHandler UpdateJobProgress;

        public override Task<ProgressResult> UpdateProgress(ProgressUpdate request, ServerCallContext context)
        {
            UpdateJobProgress?.Invoke(request.JobId, request.Message, request.CurrentProgress, request.TotalProgress);

            return Task.FromResult(new ProgressResult());
        }
    }
}
