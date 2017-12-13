using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JobManagerFramework.RemoteExecution;
using Job = JobManager.Job;
using Task = JobManagerFramework.Jenkins.Job.Task;

namespace JobManagerFramework
{
    public class RemotePool : IJobPool
    {
        private RemoteExecutionService Service { get; }

        private IDictionary<string, Job> PendingJobs { get; }

        private Thread JobMonitorThread { get; }
        private ManualResetEvent ShutdownPool { get; }

        public RemotePool(string remoteServerUri, string username, string password)
        {
            Service = new RemoteExecutionService(remoteServerUri, username, password);
            PendingJobs = new ConcurrentDictionary<string, Job>();
            ShutdownPool = new ManualResetEvent(false);
            JobMonitorThread = new Thread(MonitorJobs);
            JobMonitorThread.Start();
        }

        public void Dispose()
        {
            // TODO: implement this
            ShutdownPool.Set();
        }

        public void EnqueueJob(Job j)
        {
            // TODO: ZIP the job instead of using placeholder
            try
            {
                j.Status = Job.StatusEnum.ZippingPackage;
                string zipFilename = ZipWorkingDirectory(j.WorkingDirectory);
                j.Status = Job.StatusEnum.UploadPackage;
                string zipArtifactId = "";
                using (var fileStream = File.OpenRead(zipFilename))
                {
                    zipArtifactId = Service.UploadArtifact(fileStream);
                }

                var runCommand = "";

                if (File.Exists(Path.Combine(j.WorkingDirectory, "testbench_manifest.json")))
                {
                    // TODO: How do we want to specify which Python to use?
                    runCommand = "\"" + META.VersionInfo.PythonVEnvExe + "\"" +
                                 " -m testbenchexecutor --detailed-errors testbench_manifest.json";
                }
                else
                {
                    runCommand = j.RunCommand;
                }

                var relativeWorkingDirectory = new DirectoryInfo(j.WorkingDirectory).Name;

                var newJobId = Service.CreateJob(runCommand, relativeWorkingDirectory, zipArtifactId);
                PendingJobs.Add(newJobId, j);
                j.Status = Job.StatusEnum.PostedToServer;
            }
            catch (ZipFailedException e)
            {
                j.Status = Job.StatusEnum.Failed;
            }
            catch (RemoteExecutionService.RequestFailedException e)
            {
                j.Status = Job.StatusEnum.FailedToUploadServer;
            }
            catch (Exception)
            {
                j.Status = Job.StatusEnum.FailedToUploadServer;
            }
        }

        public bool AbortJob(Job j)
        {
            return false;
        }

        public int GetNumberOfUnfinishedJobs()
        {
            return PendingJobs.Count;
        }

        private void MonitorJobs()
        {
            while (ShutdownPool.WaitOne(0) == false)
            {
                foreach (var jobPair in PendingJobs)
                {
                    try
                    {
                        var jobStatus = Service.GetJobInfo(jobPair.Key);

                        switch (jobStatus.Status)
                        {
                            case RemoteExecutionService.RemoteJobState.Created:
                                jobPair.Value.Status = Job.StatusEnum.QueuedOnServer;
                                break;
                            case RemoteExecutionService.RemoteJobState.Running:
                                jobPair.Value.Status = Job.StatusEnum.RunningOnServer;
                                break;
                            case RemoteExecutionService.RemoteJobState.Succeeded:
                                jobPair.Value.Status = Job.StatusEnum.Succeeded;
                                break;
                            case RemoteExecutionService.RemoteJobState.Failed:
                                jobPair.Value.Status = Job.StatusEnum.Failed;
                                break;
                            case RemoteExecutionService.RemoteJobState.Cancelled:
                                jobPair.Value.Status = Job.StatusEnum.FailedAbortOnServer;
                                break;
                        }

                        //TODO: on completion, fetch and extract the completed ZIP
                        if (IsJobStateCompleted(jobStatus.Status))
                        {
                            var job = jobPair.Value;
                            PendingJobs.Remove(jobPair.Key);

                            if (!string.IsNullOrEmpty(jobStatus.ResultZipId))
                            {
                                try
                                {
                                    DownloadWorkspaceZip(job.WorkingDirectory, jobStatus.ResultZipId);
                                    UnzipWorkingDirectory(job.WorkingDirectory);
                                }
                                catch (RemoteExecutionService.RequestFailedException)
                                {
                                    job.Status = Job.StatusEnum.FailedToDownload;
                                }
                                catch (ZipFailedException)
                                {
                                    job.Status = Job.StatusEnum.FailedToDownload;
                                }
                                catch (Exception)
                                {
                                    job.Status = Job.StatusEnum.FailedToDownload;
                                }
                            }
                        }
                    }
                    catch (RemoteExecutionService.RequestFailedException)
                    {
                        jobPair.Value.Status = Job.StatusEnum.Failed;
                        var job = jobPair.Key;
                        PendingJobs.Remove(jobPair.Key);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private bool IsJobStateCompleted(RemoteExecutionService.RemoteJobState state)
        {
            switch (state)
            {
                case RemoteExecutionService.RemoteJobState.Created:
                case RemoteExecutionService.RemoteJobState.Running:
                    return false;
                case RemoteExecutionService.RemoteJobState.Succeeded:
                case RemoteExecutionService.RemoteJobState.Failed:
                case RemoteExecutionService.RemoteJobState.Cancelled:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        /**
         * Zips the working directory; returns the path to the ZIP file if successful
         */
        private string ZipWorkingDirectory(string workingDirectory)
        {
            // zip working directory
            string zipFile = Path.Combine(workingDirectory, "source_data.zip");
            string zipPy = Path.Combine(workingDirectory, "zip.py");

            // if zip.py does not exist create
            if (File.Exists(zipPy) == false)
            {
                using (StreamWriter writer = new StreamWriter(zipPy))
                {
                    writer.WriteLine(@"#!/usr/bin/python

import zipfile
import sys
import os
import os.path

path_join = os.path.join
if sys.platform == 'win32':
    def path_join(*args):
        return '\\\\?\\' + os.path.join(os.getcwd(), os.path.join(*args))

output_filename = 'source_data.zip'

if os.path.exists(output_filename):
    os.remove(output_filename)

# LS_Dyna workers have RHEL6. RHEL6 has Python2.6, which doesnt have zipfile.ZipFile.__exit__ http://bugs.python.org/issue5511 . So we dont use 'with'
z = zipfile.ZipFile(output_filename, 'w', allowZip64=True)
try:
    parent_dir_name = os.path.basename(os.getcwd())
    os.chdir('..')
    for dirpath, dirs, files in os.walk(parent_dir_name):
    # Fix META-1850: make sure all dirs are copied.
      for d in dirs:
        dn = path_join(dirpath, d)
        z.write(dn, arcname=os.path.join(dirpath, d), compress_type=zipfile.ZIP_DEFLATED)
      for f in files:
        if output_filename == f:
            continue
        fn = path_join(dirpath, f)
        #print fn
        z.write(fn, arcname=os.path.join(dirpath, f), compress_type=zipfile.ZIP_DEFLATED)
finally:
    z.close()
");
                }
            }

            if (File.Exists(zipFile) == false)
            {
                // call zip.py to zip the package if it does not exist
                ProcessStartInfo psi = new ProcessStartInfo(META.VersionInfo.PythonVEnvExe)
                {
                    Arguments = "-E \"" + zipPy + "\"",
                    WorkingDirectory = workingDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                };

                Process proc = new Process()
                {
                    StartInfo = psi,
                };

                proc.Start();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    throw new ZipFailedException(String.Format("zip.py failed with exit code {0}. stderr was {1}", proc.ExitCode, stderr));
                }
            }

            if (File.Exists(zipFile) == false)
            {
                throw new ZipFailedException(String.Format("zip.py did not produce {0}", zipFile));
            }

            return zipFile;
        }

        private void DownloadWorkspaceZip(string workingDirectory, string artifactId)
        {
            using (var writeStream = File.Create(Path.Combine(workingDirectory, "workspace.zip")))
            {
                Service.DownloadArtifact(artifactId, writeStream);
            }
        }

        // NOTE: assumes ZIP is already in working directory, as 'workspace.zip'
        private void UnzipWorkingDirectory(string workingDirectory)
        {
            // unzip package
            string unzipPy = Path.Combine(workingDirectory, "unzip.py");
            if (File.Exists(unzipPy) == false)
            {
                using (StreamWriter writer = new StreamWriter(unzipPy))
                {
                    writer.WriteLine(@"#!/usr/bin/py
import os
import os.path
import sys
import shutil
import zipfile

path_join = os.path.join
if sys.platform == 'win32':
    def path_join(*args):
        return '\\\\?\\' + os.path.join(os.getcwd(), os.path.join(*args))
try:
    parent_dir_name = os.path.basename(os.getcwd())

    zip = zipfile.ZipFile('workspace.zip')

    # OLD version zip.namelist()[0] is unpredictable
    #root_src_dir = zip.namelist()[0] + parent_dir_name
    # ASSUMPTION workspace.zip has always the parent_dir_name as a zipped directory
    root_src_dir = parent_dir_name

    print root_src_dir
    for entry in zip.infolist():
        if entry.filename.startswith(root_src_dir):
            dest = entry.filename[len(root_src_dir)+1:]
            if dest == '':
                continue
            if dest.endswith('/'):
                if not os.path.isdir(dest):
                    os.mkdir(dest)
            else:
                if os.path.basename(dest) != 'workspace.zip':
                    entry.filename = dest
                    zip.extract(entry, path=path_join(os.getcwd()))
except Exception as msg:
    import traceback
    sys.stderr.write(traceback.format_exc())
    with open('_FAILED.txt', 'wb') as f_out:
        f_out.write(str(msg))
        f_out.write('\nMost likely due to a too long file-path for Windows (max 260).')
    if os.name == 'nt':
        os._exit(3)
    elif os.name == 'posix':
        os._exit(os.EX_OSFILE)

");
                }
            }

            // call unzip.py to unzip the package
            ProcessStartInfo psi = new ProcessStartInfo(META.VersionInfo.PythonVEnvExe)
            {
                Arguments = "-E \"" + unzipPy + "\"",
                WorkingDirectory = workingDirectory,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            Process proc = new Process()
            {
                StartInfo = psi,
            };

            proc.Start();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                string logFilename = System.IO.Path.Combine(workingDirectory, LocalPool.Failed);
                File.WriteAllText(logFilename, "unzip.py failed:\n" + stderr);
                Trace.TraceError("unzip.py failed {0}", workingDirectory);
                throw new ZipFailedException("unzip.py failed");
            }
            else
            {
                if (File.Exists(System.IO.Path.Combine(workingDirectory, LocalPool.Failed)))
                {
                    // TODO: _FAILED file exists; mark as failed in job manager
                }
            }
        }

        private class ZipFailedException : Exception
        {
            public ZipFailedException(string message) : base(message) { }
        }
    }
}
