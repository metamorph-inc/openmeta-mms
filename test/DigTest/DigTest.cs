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
using System.Runtime.InteropServices;
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

        public static class KnownFolder
        {
            public static readonly Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);


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

        [Fact(Skip = "OldVisualizer")]
        void OldVisualizer()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };

            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
                options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                /*try
                {*/
                    wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/mergedPET.csv"));

                    driver.Navigate().GoToUrl(wrapper.url);
                    
                    IWait<IWebDriver> wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));
                    Assert.True(wait0.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));

                    Assert.Equal("Visualizer", driver.Title);
                    
                    /*                              PAIRS TAB                              */

                    // Check coloring schemes
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

                    driver.FindElement(By.CssSelector("a[data-value=\"Single Plot\"]")).Click();
                    IWait<IWebDriver> single_wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    
                    // Perform plot brush sequence and color data accordingly
                    Actions builder = new Actions(driver);
                    
                    IWebElement single_plot = driver.FindElement(By.CssSelector("div#singlePlot.shiny-plot-output.shiny-bound-output"));
                    //Assert.True(single_wait0.Until(driver1 => single_plot.Displayed));
                    Assert.True(single_wait0.Until(driver1 => single_plot.FindElement(By.CssSelector("img")).Displayed));

                    IAction plotBrush = builder.MoveToElement(single_plot, 80, 66).ClickAndHold().MoveByOffset(400, 400).Release().Build();

                    plotBrush.Perform();

                    driver.FindElement(By.Id("highlightData")).Click();

                    Thread.Sleep(1000);
                    Trace.WriteLine(driver.FindElement(By.Id("stats")).Text);

                    /*IWait<IWebDriver> single_wait2 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    Assert.True(single_wait2.Until(driver1 => driver.FindElement(By.Id("stats")).Displayed));
                    Assert.True(single_wait2.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Highlighted Points: 1911")));*/

                    /*                           DATA TABLE TAB                             */

                    /*  Rank data table by modelica.jturbine, select first data point, and color */
                    driver.FindElement(By.CssSelector("a[data-value=\"Data Table\"]")).Click();
                    IWebElement activateSimpleRanking = driver.FindElement(By.CssSelector("input[value=\"Simple Metric w/ TxFx\"]"));
                    Thread.Sleep(50);
                    activateSimpleRanking.Click();

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


                    /*                             PET CONFIG TAB                              */

                    // Check refined and original ranges
                    driver.FindElement(By.CssSelector("a[data-value=\"PET Refinement\"]")).Click();

                    IWait<IWebDriver> pet_wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                    Assert.True(pet_wait0.Until(driver1 => driver.FindElement(By.CssSelector("button#applyAllOriginalNumeric.btn.btn-default.action-button.shiny-bound-input")).Displayed));

                    driver.FindElement(By.CssSelector("button#applyAllOriginalNumeric.btn.btn-default.action-button.shiny-bound-input")).Click();
                    IWait<IWebDriver> wait9 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    Assert.Equal("80", wait9.Until(driver1 => driver.FindElement(By.Id("newMin2")).GetAttribute("value")));
                    
                    driver.FindElement(By.CssSelector("button#applyAllRefinedNumeric.btn.btn-default.action-button.shiny-bound-input")).Click();
                    IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    Assert.True(wait10.Until(driver1 => driver.FindElement(By.Id("newMax3")).GetAttribute("value") == "1.499990966"));

                    /*                             OPTIONS TAB                              */

                    // Get path of downloads folder
                    string downloads;
                    SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out downloads);

                    // Export and import a session
                    driver.FindElement(By.CssSelector("a[data-value=\"Options\"]")).Click();

                    String path = Path.Combine(downloads, "DigTestSettings.csv");

                    // Delete current instance of session file
                    if(File.Exists(path))
                        File.Delete(path);
                    
                    // Save Current Session
                    driver.FindElement(By.Id("sessionName")).Click();
                    driver.FindElement(By.Id("sessionName")).SendKeys("DigTestSettings");
                    Thread.Sleep(50); // Wait for text field to populate
                    driver.FindElement(By.Id("exportSession")).Click();

                    // Change data coloring to Max/Min in pairs tab
                    driver.FindElement(By.CssSelector("a[data-value=\"Pairs Plot\"]")).Click();
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Ranked\"]")).Click());
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Max/Min\"]")).Click());

                    // Load earlier session
                    driver.FindElement(By.CssSelector("a[data-value=\"Options\"]")).Click();
                    IWait<IWebDriver> option_wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                    Assert.True(option_wait0.Until(driver1 => driver.FindElement(By.Id("loadSessionName")).Displayed));
                    driver.FindElement(By.Id("loadSessionName")).Click();
                    driver.FindElement(By.Id("loadSessionName")).SendKeys(path);
                    driver.FindElement(By.Id("importSession")).Click();

                    // Check to see that coloring mode has returned to Ranked
                    Assert.True(wait8.Until(driver1 => driver.FindElement(By.Id("stats")).Text.Contains("Ranked Points: 1")));

                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Ranked\"]")).Click());
                    RetryStaleElement(() => driver.FindElement(By.CssSelector("div[data-value=\"Highlighted\"]")).Click());


                /*}
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        // this should keep the browser open for inspection
                        Debugger.Break();
                    }
                    throw;
                }*/

            }

        }

        [Fact()]
        void DigRuns()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };
            File.Copy(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config.json"),
                      Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"),
                      overwrite: true);
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
                options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"));
                driver.Navigate().GoToUrl(wrapper.url);
                IWait<IWebDriver> wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));
                Assert.True(wait0.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));
                Assert.Equal("Visualizer", driver.Title);
                IWait<IWebDriver> wait1 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                Assert.True(wait1.Until(driver1 => driver.FindElement(By.Id("Explore-pairs_stats")).Text.Contains("Total Points: 5000")));
            }
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"));
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/raw_data_aug.csv"));
        }

        [Fact()]
        void SessionRestore()
        {
            // TODO(tthomas): Add testing of additional UI elements
            
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            File.Copy(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config.json"),
                      Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"),
                      overwrite: true);
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
                options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                // Open and make changes
                wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"));
                driver.Navigate().GoToUrl(wrapper.url);
                IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                Assert.True(wait10.Until(driver1 => driver.FindElement(By.XPath("//*[@id='Explore-pairs_plot']/img")).Displayed));
                IWebElement display_select = driver.FindElement(By.XPath("//select[@id='Explore-display']/following::div"));
                display_select.Click();
                IWebElement display_input = display_select.FindElement(By.XPath("//input"));
                display_input.SendKeys(Keys.ArrowRight);
                display_input.SendKeys("rho");
                display_input.SendKeys(Keys.Enter);
                display_input.SendKeys(Keys.ArrowRight);
                display_input.SendKeys("PowerOutput");
                display_input.SendKeys(Keys.Enter);
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='modelica.jturbine']")));
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='modelica.ratio']")));
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='modelica.rho']")));
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='tl_peakPowerOutput.output']")));
                Thread.Sleep(300); //For shiny to catch up, find a better way
                driver.Close();
            }
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
               options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                // Reload to check changes
                wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"));
                driver.Navigate().GoToUrl(wrapper.url);
                IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                Assert.True(wait10.Until(driver1 => driver.FindElement(By.XPath("//*[@id='Explore-pairs_plot']/img")).Displayed));
                Assert.Equal(driver.FindElements(By.XPath(".//select[@id='Explore-display']/../div/div[1]/div")).Count, 4);
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='modelica.jturbine']")));
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='modelica.ratio']")));
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='modelica.rho']")));
                wait10.Until(ExpectedConditions.ElementExists(By.XPath("//select[@id='Explore-display']/following::div/div//div[@data-value='tl_peakPowerOutput.output']")));
                driver.Close();
            }
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/visualizer_config_test.json"));
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbine/raw_data_aug.csv"));
        }

        [Fact()]
        void UncertaintyQuantification()
        {
            File.Copy(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbineForOptimization/visualizer_config.json"),
                      Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbineForOptimization/visualizer_config_test.json"),
                      overwrite: true);
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(
                options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                IWait<IWebDriver> wait2 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(2.0));
                IWait<IWebDriver> wait5 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                IWait<IWebDriver> wait30 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));

                wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbineForOptimization/visualizer_config_test.json"));
                driver.Navigate().GoToUrl(wrapper.url);
                Assert.True(wait5.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));
                Assert.Equal("Visualizer", driver.Title);

                driver.FindElement(By.XPath("//a[@data-value=\"Uncertainty Quantification\"]")).Click();
                driver.FindElement(By.Id("UncertaintyQuantification-design_configs_present")).Click();
                Assert.True(wait5.Until(driver1 => driver.FindElement(By.XPath("//div[@id='UncertaintyQuantification-vars_plots']/div[1]/div/div/img")).Displayed));
                Assert.True(wait2.Until(driver1 => driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/../../../..")).Displayed));
                driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/following-sibling::div")).Click();
                Assert.True(wait5.Until(driver1 => driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/following-sibling::div/div[2]")).Displayed));
                driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/following-sibling::div/div[2]//div[@data-value='32-16']")).Click();
                Assert.True(wait5.Until(driver1 => driver.FindElement(By.XPath("//div[@id='UncertaintyQuantification-vars_plots']/div[1]/div/div/img")).Displayed));

                // Forward UQ
                driver.FindElement(By.Id("UncertaintyQuantification-fuq_constraint_enable2")).Click();
                Assert.True(wait2.Until(driver1 => driver.FindElement(By.Id("UncertaintyQuantification-fuq_constraint_enable2")).Selected));
                driver.FindElement(By.Id("UncertaintyQuantification-run_forward_uq")).Click();
                Assert.True(wait5.Until(driver1 => driver.FindElement(By.XPath("//div[@id='UncertaintyQuantification-vars_plots']/div[1]/div/div/img")).Displayed));

                //// Add Probability Query
                driver.FindElement(By.Id("UncertaintyQuantification-add_probability")).Click();
                Assert.True(wait5.Until(driver1 => driver.FindElement(By.Id("UncertaintyQuantification-queryThreshold0")).Displayed));

                driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-queryVariable0']/following-sibling::div")).Click();
                Assert.True(wait2.Until(driver1 => driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-queryVariable0']/following-sibling::div/div[2]")).Displayed));
                driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-queryVariable0']/following-sibling::div/div[2]//div[@data-value='OUT_Blade_Tip_Deflection']")).Click();

                

                //driver.FindElement(By.XPath("//select[@data-value='OUT_Blade_Tip_Deflection']")).Click();

                driver.FindElement(By.Id("UncertaintyQuantification-queryThreshold0")).SendKeys("2400");
                Assert.True(wait2.Until(driver1 => driver.FindElement(By.CssSelector("#UncertaintyQuantification-queryThreshold0")).GetAttribute("value") == "2400"));
                    
                //// Evaluate current probability Query
                driver.FindElement(By.Id("UncertaintyQuantification-run_probabilities_queries")).Click();
                Assert.True(wait2.Until(driver1 => driver.FindElement(By.Id("UncertaintyQuantification-queryValue0")).Displayed));
                Assert.True(wait2.Until(driver1 => float.Parse(driver.FindElement(By.Id("UncertaintyQuantification-queryValue0")).Text) < 0.35));

                ///*      DESIGN RANKING TAB      */
                //driver.FindElement(By.CssSelector("#uqTabset > li:nth-child(2) > a")).Click();

                //IWait<IWebDriver> UQ_wait4 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                //Assert.True(UQ_wait4.Until(driver1 => driver.FindElement(By.CssSelector("#runProbability")).Displayed));
                //driver.FindElement(By.CssSelector("#runProbability")).Click();

                ///*
                //IWait<IWebDriver> UQ_wait5 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                //Assert.True(UQ_wait5.Until(driver1 => driver.FindElement(By.CssSelector("#probabilityTable")).Displayed));
                //*/
            }
        }

        class DigWrapper : IDisposable
        {
            public StringBuilder stdoutData = new StringBuilder();
            public StringBuilder stderrData = new StringBuilder();
            public string url;
            Process proc;

            public void Start(string input_json)
            {
                Process proc = new Process();
                proc.StartInfo.Arguments = "--no-save --no-restore -e \"shiny::runApp('Dig',display.mode='normal',quiet=FALSE, launch.browser=FALSE)\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = Path.Combine(META.VersionInfo.MetaPath, @"bin\R\bin\x64\Rscript.exe");
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.WorkingDirectory = Path.Combine(META.VersionInfo.MetaPath, @"bin");
                //proc.StartInfo.EnvironmentVariables["DIG_INPUT_CSV"] = input_csv;
                proc.StartInfo.EnvironmentVariables["DIG_DATASET_CONFIG"] = input_json;
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
                if (proc != null && !proc.WaitForExit(1000))
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
