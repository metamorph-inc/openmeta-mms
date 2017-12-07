using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
