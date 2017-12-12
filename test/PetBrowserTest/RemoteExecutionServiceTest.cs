using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using JobManagerFramework.RemoteExecution;

namespace PetBrowserTest
{
    public class RemoteExecutionServiceTest
    {
        [Fact]
        public void TestRuns()
        {
            Assert.True(true);
        }

        [Fact]
        public void GetJobGetsJob()
        {
            var service = new RemoteExecutionService("http://localhost:8080", "test", "test");

            var job = service.GetJobInfo("9be66783-8805-5242-9f34-a603a051ae24");

            Assert.NotNull(job);
            Assert.Equal(job.Uid, "9be66783-8805-5242-9f34-a603a051ae24");
        }

        [Fact]
        public void GetNonexistentJobThrows()
        {
            var service = new RemoteExecutionService("http://localhost:8080", "test", "test");

            Assert.Throws<RemoteExecutionService.ObjectNotFoundException>(() => service.GetJobInfo("not_a_job_id"));
        }

        [Fact]
        public void TestArtifactUpload()
        {
            var service = new RemoteExecutionService("http://localhost:8080", "test", "test");

            var hash = "";

            using (var uploadedFileStream = new MemoryStream(Resources.test_run_dir))
            {
                hash = service.UploadArtifact(uploadedFileStream);
            }

            Assert.Equal(hash, "4a0c2b277e4451d5ac59ccd57edf786c79455c99");
        }

        [Fact]
        public void TestArtifactDownload()
        {
            var service = new RemoteExecutionService("http://localhost:8080", "test", "test");

            var hash = "";

            using (var uploadedFileStream = new MemoryStream(Resources.test_run_dir))
            {
                hash = service.UploadArtifact(uploadedFileStream);
            }

            Assert.Equal(hash, "4a0c2b277e4451d5ac59ccd57edf786c79455c99");

            using (var downloadedFileStream = new MemoryStream())
            {
                service.DownloadArtifact("4a0c2b277e4451d5ac59ccd57edf786c79455c99", downloadedFileStream);

                Assert.Equal(downloadedFileStream.GetBuffer(), Resources.test_run_dir);
            }
        }

        [Fact]
        public void TestRunJob()
        {
            var service = new RemoteExecutionService("http://localhost:8080", "test", "test");

            var hash = "";

            using (var uploadedFileStream = new MemoryStream(Resources.test_run_dir))
            {
                hash = service.UploadArtifact(uploadedFileStream);
            }

            Assert.Equal(hash, "4a0c2b277e4451d5ac59ccd57edf786c79455c99");

            var jobId = service.CreateJob("dir", hash);
            Assert.NotNull(jobId);
        }

        /**
         * Note: you'll need both an executor service and worker running for this test method
         * to complete successfully
         */
        [Fact]
        public void TestRunJobToCompletion()
        {
            var service = new RemoteExecutionService("http://localhost:8080", "test", "test");

            var hash = "";

            using (var uploadedFileStream = new MemoryStream(Resources.test_run_dir))
            {
                hash = service.UploadArtifact(uploadedFileStream);
            }

            Assert.Equal(hash, "4a0c2b277e4451d5ac59ccd57edf786c79455c99");

            var jobId = service.CreateJob("dir", hash);
            Assert.NotNull(jobId);

            var jobComplete = false;
            var timer = 0;

            do
            {
                var jobStatus = service.GetJobInfo(jobId);

                if (jobStatus.Status == RemoteExecutionService.RemoteJobState.Succeeded ||
                    jobStatus.Status == RemoteExecutionService.RemoteJobState.Cancelled ||
                    jobStatus.Status == RemoteExecutionService.RemoteJobState.Failed)
                {
                    jobComplete = true;
                }
                else
                {
                    Thread.Sleep(1000);
                    timer += 1000;
                }
            } while (!jobComplete && timer <= (20 * 1000));

            var finalStatus = service.GetJobInfo(jobId);

            Assert.Equal(RemoteExecutionService.RemoteJobState.Succeeded, finalStatus.Status);
            Assert.NotNull(finalStatus.ResultZipId);

            Assert.Equal(hash, "4a0c2b277e4451d5ac59ccd57edf786c79455c99");

            using (var downloadedFileStream = new MemoryStream())
            {
                service.DownloadArtifact(finalStatus.ResultZipId, downloadedFileStream);

                Assert.NotEqual(0, downloadedFileStream.Length);
            }
        }
    }
}
