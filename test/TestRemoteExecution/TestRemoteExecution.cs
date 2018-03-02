using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobManager;
using JobManagerFramework;
using System.Diagnostics;
using System.IO;
using GME.MGA;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.InteropServices;
using System.Data.EntityClient;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace TestRemoteExecution
{
    [Guid("7C908995-E367-4D93-B112-24EC52ED6F69")]
    [ProgId("CyPhy.TestRemoteExecution"),
    ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class TestRemoteExecution : IDisposable
    {
        private JobManagerFramework.JobManager Manager { get; set; }
        Process server;
        ManualResetEvent serverListening;
        Process worker;
        BlockingCollection<Job> jobUpdates = new BlockingCollection<Job>();

        private int port = 64231;
        public string remoteServerUri;
        public string username = "test_remote_execution";
        public string password;
        private IntPtr win32job = CyPhyGUIs.JobObjectPinvoke.CreateKillOnCloseJob();

        public TestRemoteExecution()
        {
            remoteServerUri = "http://127.0.0.1:" + port + "/";
            // This code takes the place of this app.config line, which COM clients won't have
            // <add name="SQLite Data Provider" invariant="System.Data.SQLite" description=".Net Framework Data Provider for SQLite" type="System.Data.SQLite.SQLiteFactory, System.Data.SQLite"/>
            var systemData = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
            //Ensure table exists
            if (systemData.Tables.IndexOf("DbProviderFactories") > -1)
            {
                //remove existing provider factory
                if (systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Find("System.Data.SQLite") != null)
                {
                    systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Remove(
                        systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Find("System.Data.SQLite"));
                }
            }
            else
            {
                systemData.Tables.Add("DbProviderFactories");
            }
            //Add provider factory with our assembly in it.
            systemData.Tables[systemData.Tables.IndexOf("DbProviderFactories")].Rows.Add("SQLite Data Provider"
                , ".NET Framework Data Provider for SQLite"
                , "System.Data.SQLite"
                , "System.Data.SQLite.SQLiteFactory, System.Data.SQLite"
            );

            // CLR doesn't know where to look for SQLite dll in the EntityFramework code
            var sqliteAssembly = new System.Data.SQLite.SQLiteCommand().GetType().Assembly;
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                string assemblyFile = (args.Name.Contains(','))
                    ? args.Name.Substring(0, args.Name.IndexOf(','))
                    : args.Name;

                assemblyFile += ".dll";

                if (assemblyFile.Equals(Path.GetFileName(sqliteAssembly.Location), StringComparison.InvariantCultureIgnoreCase))
                {
                    return Assembly.LoadFile(sqliteAssembly.Location);
                }
                return null;
            };

            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var random = new byte[20];
            rng.GetBytes(random);
            password = Encoding.UTF8.GetString(random.Select(b => (byte)(' ' + (b % ('~' - ' ')))).ToArray());
            // passwords with colons do not work https://github.com/izaakschroeder/express-authentication-basic/issues/8
            password = password.Replace(":", "a");
            // Console.Out.WriteLine("Password: " + password);

            if (File.Exists("auth.json"))
            {
                // File.Delete("auth.json");
            }
        }

        public void AddUser()
        {
            var addUserInfo = new ProcessStartInfo("openmeta-executor-server.exe", String.Format("add-user \"{0}\"", username));
            Process addUser = new Process();
            addUser.StartInfo = addUserInfo;
            addUserInfo.RedirectStandardInput = true;
            addUserInfo.UseShellExecute = false;
            addUser.Start();
            CyPhyGUIs.JobObjectPinvoke.AssignProcessToJobObject(addUser, win32job);
            addUser.StandardInput.WriteLine(password);
            addUser.StandardInput.Close();
            addUser.WaitForExit();
            if (addUser.ExitCode != 0)
            {
                throw new ApplicationException("Error adding user to openmeta-executor-server");
            }
        }

        public void StartServer()
        {
            var info = new ProcessStartInfo("openmeta-executor-server.exe", String.Format("--listen-address 127.0.0.1 --listen-port {0}", port));
            server = new Process();
            server.StartInfo = info;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            serverListening = new ManualResetEvent(false);
            server.OutputDataReceived += (e, data) =>
            {
                if (data.Data != null)
                {
                    if (data.Data.Contains("Listening on "))
                    {
                        serverListening.Set();
                    }
                    Console.Out.WriteLine(data.Data);
                }
            };
            server.ErrorDataReceived += (e, data) =>
            {
                if (data.Data != null)
                {
                    Console.Error.WriteLine(data.Data);
                }
            };
            server.Start();
            CyPhyGUIs.JobObjectPinvoke.AssignProcessToJobObject(server, win32job);
            server.StandardInput.Close();
            server.BeginErrorReadLine();
            server.BeginOutputReadLine();
            // server.WaitForExit();
            // Assert.Equal(0, server.ExitCode);
        }

        public void StartWorker()
        {
            string workerKey;
            var auth = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("auth.json"));
            workerKey = (string)auth["workerKey"];

            if (serverListening.WaitOne(10 * 1000) == false)
            {
                throw new TimeoutException("Timeout waiting for server to listen");
            }

            var info = new ProcessStartInfo("openmeta-executor-worker.exe", String.Format(
                // FIXME get version number better, or pass --ignore-job-labels to server
                "-l Windows14.13,OpenModelica,py_modelica14.13,OpenModelica__latest_,Creo,CADCreoParametricCreateAssembly.exev1.4,Schematic,Simulink,RF,Visualizer, " +
                "{0} \"{1}\"", remoteServerUri, workerKey));
            worker = new Process();
            worker.StartInfo = info;
            info.EnvironmentVariables["PATH"] =
                META.VersionInfo.PythonVEnvPath + "\\Scripts;" +
                Environment.GetEnvironmentVariable("PATH");
            info.RedirectStandardInput = true;
            info.UseShellExecute = false;
            worker.Start();
            CyPhyGUIs.JobObjectPinvoke.AssignProcessToJobObject(worker, win32job);
            worker.StandardInput.Close();
        }

        public void StartJobManager()
        {
            Manager = new JobManagerFramework.JobManager(localConcurrentThreads: 1);
            Manager.Server.IsRemote = false;
            Manager.LoadSavedJobs();
            Manager.JobAdded += Manager_JobAdded;
            Manager.JobCollectionAdded += JobCollectionAddedHandler;
            Manager.SwitchToRemotePool(remoteServerUri, username, password);

            // Manager.ReRunJobs(new [] {j });
            // Manager.AbortJobs(new [] {j });
            // Manager.SwitchToLocalPool(SelectedThreadCount);
        }

        private void Manager_JobAdded(object sender, JobManagerFramework.JobManager.JobAddedEventArgs e)
        {
            ((JobImpl)e.Job).JobStatusChanged += (job, status) =>
            {
                jobUpdates.Add(job);
            };
        }
        private void JobCollectionAddedHandler(object o, JobManagerFramework.JobManager.JobCollectionAddedEventArgs jobCollectionAddedEventArgs)
        {
        }

        CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI masterApi;
        MgaProject project;
        public void OpenProject(string mgaFile)
        {
            if (project != null)
            {
                project.Close(true);
            }
            project = new MgaProjectClass();
            project.OpenEx("MGA=" + mgaFile, "CyPhyML", null);
        }

        public CyPhyMasterInterpreter.MasterInterpreterResult Test(string tbpath, string configPath, int jobRunTimeout)
        {
            string tbId;
            string configId;

            project.BeginTransactionInNewTerr();
            try
            {
                var tb = project.RootFolder.ObjectByPath[tbpath];
                if (tb == null)
                {
                    throw new ApplicationException("Could not find " + tbpath);
                }
                tbId = tb.ID;
                var config = project.RootFolder.ObjectByPath[configPath];
                if (config == null)
                {
                    throw new ApplicationException("Could not find " + configPath);
                }
                configId = config.ID;
            }
            finally
            {
                project.AbortTransaction();
            }


            var configurationSelection = new CyPhyMasterInterpreter.ConfigurationSelectionLight()
            {
                PostToJobManager = true,
                ContextId = tbId,
                SelectedConfigurationIds = new string[] { configId }
            };
            if (masterApi != null)
            {
                masterApi.Dispose();
            }
            masterApi = new CyPhyMasterInterpreter.CyPhyMasterInterpreterAPI();
            masterApi.Initialize(project);
            var results = masterApi.RunInTransactionWithConfigLight(configurationSelection);
            if (results.First().Success == false)
            {
                throw new ApplicationException("MasterInterpreter run failed");
            }

            while (true)
            {
                Job job;
                if (jobUpdates.TryTake(out job, jobRunTimeout))
                {
                    if (Job.IsFailedStatus(job.Status))
                    {
                        // TODO dump logs, stdout.txt, stderr.txt
                        throw new ApplicationException("Remote run failed: " + job.Status);
                    }
                    else if (job.Status == Job.StatusEnum.Succeeded)
                    {
                        break;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            return results[0];
        }

        public void Dispose()
        {
            if (masterApi != null)
            {
                masterApi.Dispose();
            }
            if (Manager != null)
            {
                Manager.Dispose();
            }
            if ((project.ProjectStatus & 1) != 0)
            {
                project.Close(true);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") +
                ";" + @"C:\Users\kevin\Documents\openmeta-executor\_server" +
                ";" + @"C:\Users\kevin\Documents\openmeta-executor\_worker");

            string tbpath = "/@Testing/@Test_benches/@Testing/@truck";
            string configPath = "/@Designs/@SimpleSystem";
            string mgaFile = @"C:\Users\kevin\Documents\tonka\models\PET_simple_proof-of-concept\WorkFlow_PET.mga";

            var test = new TestRemoteExecution();
            test.AddUser();
            test.StartServer();
            test.StartWorker();
            test.StartJobManager();

            test.OpenProject(mgaFile);

            test.Test(tbpath, configPath, 10 * 1000);

            test.Dispose();
        }
    }


}
