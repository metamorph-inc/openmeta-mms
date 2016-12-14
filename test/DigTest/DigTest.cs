using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
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

        void RetryStaleElement(Action a)
        {
            int tries = 3;
            while (true)
            {
                try
                {
                    a();
                    break;
                }
                catch (OpenQA.Selenium.StaleElementReferenceException)
                {
                    if (--tries == 0)
                    {
                        throw;
                    }
                }
            }
        }

        void RetryNoSuchElement(Action a)
        {
            int tries = 3;
            while (true)
            {
                try
                {
                    a();
                    break;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    if (--tries == 0)
                    {
                        throw;
                    }
                }
            }
        }

        [Fact]
        void DigRuns()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions
                {

          

        };
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
                options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                try
                {
                    wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/WindTurbineSimmerged.csv"));

                    driver.Navigate().GoToUrl(wrapper.url);
                    
                    IWait<IWebDriver> wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));
                    Assert.True(wait0.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));

                    Assert.Equal("Visualizer", driver.Title);

                    /*                              PAIRS TAB                              */

                    IWait<IWebDriver> wait1 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait1.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Total Points: 5000")));

                    /*  Min/Max Coloring   */
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"None\"]")).Click());
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Max/Min\"]")).Click());
                    IWait<IWebDriver> wait2 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait2.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Best Points: 1700")));

                    /*  Highlighted Coloring   */
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Max/Min\"]")).Click());
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Highlighted\"]")).Click());
                    IWait<IWebDriver> wait3 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait3.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Highlighted Points: 0")));

                    /*  Ranked Coloring   */
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Highlighted\"]")).Click());
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Ranked\"]")).Click());
                    IWait<IWebDriver> wait4 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait4.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Ranked Points: 0")));


                    /*                          SINGLE PLOT TAB                             */

                    

                    // Switch to single plot tab
                    driver.FindElement(By.CssSelector("a[data-value=\"Single Plot\"]")).Click();
                    IWait<IWebDriver> single_wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    
                    // Wait until plot is displayed and build plot brush sequence
                    Actions builder = new Actions(driver);
                    
                    IWebElement single_plot = driver.FindElement(By.CssSelector("div#singlePlot.shiny-plot-output.shiny-bound-output"));
                    //Assert.True(single_wait0.Until(driver1 => single_plot.Displayed));
                    Assert.True(single_wait0.Until(driver1 => single_plot.FindElement(By.CssSelector("img")).Displayed));

                    IAction plotBrush = builder.MoveToElement(single_plot, 80, 66).ClickAndHold().MoveByOffset(40, 40).Release().Build();

                    plotBrush.Perform();

                    driver.FindElement(By.Id("highlightData")).Click();

                    IWait<IWebDriver> single_wait2 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(single_wait2.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Highlighted Points: 8")));

                    /*                           DATA TABLE TAB                             */

                    /*  Rank data table by modelica.jturbine, select first data point, and color */
                    driver.FindElement(By.CssSelector("a[data-value=\"Data Table\"]")).Click();
                    IWebElement activateRanking = driver.FindElement(By.CssSelector("input#activateRanking"));
                    Thread.Sleep(50);
                    activateRanking.Click();

                    driver.FindElement(By.CssSelector("div[class=\"selectize-input items not-full has-options\"]")).Click();
                    IWait<IWebDriver> wait5 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait5.Until(driver1 => driver.FindElement(By.CssSelector("div[class=\"selectize-input items not-full has-options focus input-active dropdown-active\"]")).Displayed));
                    IWait<IWebDriver> wait6 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait6.Until(driver1 => driver.FindElement(By.CssSelector("div.option.active")).Displayed));
                    driver.FindElement(By.CssSelector("div.option.active")).Click();
                    IWait<IWebDriver> wait7 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait7.Until(driver1 => driver.FindElement(By.CssSelector("tbody > tr:nth-child(1) > td:nth-child(1)")).Text.Contains("4460")));
                    driver.FindElement(By.CssSelector("tbody > tr:nth-child(1) > td:nth-child(1)")).Click();
                    driver.FindElement(By.CssSelector("button#colorRanked.btn.btn-default.action-button.shiny-bound-input")).Click();
                    IWait<IWebDriver> wait8 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(wait8.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Ranked Points: 1")));


                    /*                             RANGES TAB                              */

                    driver.FindElement(By.CssSelector("a[data-value=\"Ranges\"]")).Click();

                    //driver.Wait (By.CssSelector("button#applyAllOriginalNumeric.btn.btn-default.action-button.shiny-bound-input")).Displayed;

                    driver.FindElement(By.CssSelector("button#applyAllOriginalNumeric.btn.btn-default.action-button.shiny-bound-input")).Click();
                    IWait<IWebDriver> wait9 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    Assert.Equal("80", wait9.Until(driver1 => driver.FindElement(By.Id("newMin2")).GetAttribute("value")));
                    
                    driver.FindElement(By.CssSelector("button#applyAllRefinedNumeric.btn.btn-default.action-button.shiny-bound-input")).Click();
                    IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    Assert.True(wait10.Until(driver1 => driver.FindElement(By.Id("newMax3")).GetAttribute("value") == "1.4999909657"));



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
