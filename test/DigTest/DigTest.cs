using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DigTest
{
    public class DigTest
    {
        static void Main(string[] args)
        {
            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                System.Reflection.Assembly.GetAssembly(typeof(DigTest)).CodeBase.Substring("file:///".Length),
                //"/noshadow",
            });
            Console.In.ReadLine();
        }

        [Fact]
        void DigRuns()
        {
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
                new OpenQA.Selenium.Chrome.ChromeOptions
                {
                }))
            using (DigWrapper wrapper = new DigWrapper())
            {
                try
                {
                    wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "deploy/CAD_Installs/Proe ISIS Extensions/docs/examples/compute_metrics_ptc/ComputeMetricsExamplemapping.csv"));
                    wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "deploy/CAD_Installs/Proe ISIS Extensions/docs/examples/compute_metrics_ptc/ComputeMetricsExamplemerged.csv"));

                    driver.Navigate().GoToUrl(wrapper.url);
                    IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));
                    Assert.True(wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));

                    Assert.Equal("Visualizer", driver.Title);

                    IWait<IWebDriver> wait3 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait3.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Total Points: 120")));

                    driver.FindElement(By.CssSelector("div[data-value=\"None\"]")).Click();
                    driver.FindElement(By.CssSelector("div[data-value=\"Max/Min\"]")).Click(); ;
                    IWait<IWebDriver> wait2 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait2.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Best Points: 30")));
                }
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        // this should keep the browser open for inspection
                        Debugger.Break();
                    }
                    throw;
                }               
                
            }

        }

        class DigWrapper : IDisposable
        {
            public StringBuilder stdoutData = new StringBuilder();
            public StringBuilder stderrData = new StringBuilder();
            public string url;
            Process proc;

            public void Start(string input_csv)
            {
                Process proc = new Process();
                proc.StartInfo.Arguments = "--no-save --no-restore -e \"shiny::runApp('Dig',display.mode='normal',quiet=FALSE, launch.browser=FALSE)\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = Path.Combine(META.VersionInfo.MetaPath, @"bin\R\bin\x64\Rscript.exe");
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.WorkingDirectory = Path.Combine(META.VersionInfo.MetaPath, @"bin");
                proc.StartInfo.EnvironmentVariables["DIG_INPUT_CSV"] = input_csv;
                ManualResetEvent task = new ManualResetEvent(false);
                using (task)
                {
                    proc.OutputDataReceived += (o, args) =>
                    {
                        if (args.Data != null)
                        {
                            lock (stdoutData)
                            {
                                stdoutData.Append(args.Data);
                            }
                        }
                    };
                    proc.ErrorDataReceived += (o, args) =>
                    {
                        if (args.Data != null)
                        {
                            lock (stderrData)
                            {
                                stderrData.Append(args.Data);
                                Console.Error.WriteLine(args.Data);
                                try
                                {
                                    if (args.Data.StartsWith("Listening on "))
                                    {
                                        url = args.Data.Substring("Listening on ".Length);
                                        task.Set();
                                    }
                                }
                                catch (ObjectDisposedException) { }
                            }
                        }
                    };
                    proc.Start();
                    this.proc = proc;
                    proc.BeginErrorReadLine();
                    proc.BeginOutputReadLine();
                    proc.StandardInput.Close();

                    var tokenSource = new CancellationTokenSource();
                    int timeOut = 10000; // ms
                    if (task.WaitOne(timeOut) == false)
                    {
                        Console.WriteLine("The Task timed out!");
                        Assert.True(false, string.Format("Did not find \"Listening on\" in Dig output. Operation timed out after {0}  ms. Stderr: {1}", timeOut, stderrData.ToString()));
                    }
                }

            }

            public void Dispose()
            {
                if (proc != null && proc.HasExited == false)
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch (System.InvalidOperationException) { } // possible race with proc.HasExited
                }

            }
        }
    }
}
