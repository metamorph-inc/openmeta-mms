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
using System.Drawing;

namespace DigTest
{
    public class DigTest
    {
        static void Main(string[] args)
        {

            int ret = Xunit.ConsoleClient.Program.Main(new string[] {
                System.Reflection.Assembly.GetAssembly(typeof(DigTest)).CodeBase.Substring("file:///".Length),
                //"/noshadow",
                "/trait", "Category=ResultsBrowser"
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
                    //Thread.Sleep(100);
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
        
        [Fact()]
        void GenericCSVLaunch()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            options.AddArgument("--start-maximized");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/GenericCSV/2010_Census_Populations_by_Zip_Code.csv"), true);
                driver.Navigate().GoToUrl(wrapper.url);
                IWait<IWebDriver> wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));
                Assert.True(wait0.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));
                Assert.Equal("Visualizer", driver.Title);
                IWait<IWebDriver> wait1 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                Assert.True(wait1.Until(driver1 => driver.FindElement(By.Id("Explore-pairs_stats")).Text.Contains("Total Points: 319")));
                Assert.True(wait1.Until(driver1 => driver.FindElement(By.Id("Explore-pairs_stats")).Text.Contains("Current Points: 316")));
            }
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/GenericCSV/2010_Census_Populations_by_Zip_Code_viz_config.json"));
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/GenericCSV/2010_Census_Populations_by_Zip_Code_viz_config_data.csv"));
        }

        [Fact()]
        void OpenmetaCSVLaunch()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            options.AddArgument("--start-maximized");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                wrapper.Start(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/OpenmetaCSV/windturbine_merged.csv"), true);
                driver.Navigate().GoToUrl(wrapper.url);
                IWait<IWebDriver> wait0 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));
                Assert.True(wait0.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));
                Assert.Equal("Visualizer", driver.Title);
                IWait<IWebDriver> wait1 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
                Assert.True(wait1.Until(driver1 => driver.FindElement(By.Id("Explore-pairs_stats")).Text.Contains("Total Points: 5000")));
            }
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/OpenmetaCSV/windturbine_merged_viz_config.json"));
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/OpenmetaCSV/windturbine_merged_viz_config_data.csv"));
        }

        [Fact()]
        [Trait("Category","ResultsBrowser")]
        void ResultsBrowserJSONLaunch()
        {
            // TODO(tthomas): Add testing of additional UI elements
            var session = new ShinySession(Path.Combine("bin", "dig", "datasets", "WindTurbineForOptimization", "visualizer_config.json"));
            File.Copy(session.original_config, session.copied_config, overwrite: true);
            File.Delete(session.log_file);
            
            var options = new OpenQA.Selenium.Chrome.ChromeOptions { };
            options.AddUserProfilePreference("auto-open-devtools-for-tabs", "true");
            options.AddArgument("--start-maximized");

            // Launch first session
            File.AppendAllText(session.log_file, "First Launch Log ------------------------\n");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                wrapper.Start(session.copied_config);
                driver.Navigate().GoToUrl(wrapper.url);
                Assert.True(ShinyUtilities.WaitUntilDocumentReady(driver));
                Assert.Equal("Visualizer", driver.Title);

                ExploreSet(driver);
                TabsSet(driver);
                FooterSet(driver);

                //Thread.Sleep(300); //For shiny to catch up, find a better way
                driver.Close();
                wrapper.AppendLog(session.log_file);
            }

            // Launch second session to ensure proper session restore
            File.AppendAllText(session.log_file, "\nSecond Launch Log ------------------------\n");
            using (IWebDriver driver = new OpenQA.Selenium.Chrome.ChromeDriver(options))
            using (DigWrapper wrapper = new DigWrapper())
            {
                wrapper.Start(session.copied_config);
                driver.Navigate().GoToUrl(wrapper.url);

                TabsCheck(driver);

                driver.Close();
                wrapper.AppendLog(session.log_file);
            }

            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbineForOptimization/visualizer_config_test.json"));
            File.Delete(Path.Combine(META.VersionInfo.MetaPath, "bin/Dig/datasets/WindTurbineForOptimization/visualizer_config_test_data.csv"));
        }

        // Test "Explore.R"
        private void ExploreSet(IWebDriver driver)
        {
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            Actions builder = new Actions(driver);

            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Explore");

            // Test Pairs Plot
            Assert.True(wait.Until(driver1 => driver.FindElement(By.XPath("//*[@id='Explore-pairs_plot']/img")).Displayed));
            var display = new ShinySelectMultipleInput(driver, "Explore-display");
            var pairs_plot = new ShinyPlot(driver, "Explore-pairs_plot");
            display.AppendSelection("OUT");
            display.AppendSelection("OUT");
            pairs_plot.WaitUntilImageRefreshes();

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Plot Options");
            var start = pairs_plot.ImageStats();
            var autorender = new ShinyCheckboxInput(driver, "Explore-auto_render");
            Assert.True(autorender.GetDefaultState());
            //TODO(tthomas): Test Delayed Render
            //Assert.False(autorender.ToggleState());
            //ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Variables");

            var upperpanel = new ShinyCheckboxInput(driver, "Explore-pairs_upper_panel");
            Assert.False(upperpanel.GetDefaultState());
            Assert.True(upperpanel.ToggleState());
            pairs_plot.WaitUntilImageRefreshes();
            Assert.True(pairs_plot.ImageStats()[Color.FromArgb(255,0,0,0)] > start[Color.FromArgb(255,0,0,0)] * 1.5);

            var trendlines = new ShinyCheckboxInput(driver, "Explore-pairs_trendlines");
            Assert.False(trendlines.GetDefaultState());
            Assert.False(start.ContainsKey(Color.FromArgb(255, 255, 0, 0)));
            Assert.True(trendlines.ToggleState());
            pairs_plot.WaitUntilImageRefreshes();
            Assert.True(pairs_plot.ImageIncludesColor(Color.FromArgb(255, 255, 0, 0)));

            var displayunits = new ShinyCheckboxInput(driver, "Explore-pairs_units");
            Assert.True(displayunits.GetDefaultState());

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Markers");
            var marker_size_pairs = new ShinySliderInput(driver, "Explore-pairs_plot_marker_size");
            var initial_count = pairs_plot.ImageStats();
            Assert.Equal(1.5, marker_size_pairs.MoveSliderToValue(1.5));
            pairs_plot.WaitUntilImageRefreshes();
            //Assert.True(pairs_plot.ImageHasChanged()); // Faster Method
            var second_count = pairs_plot.ImageStats();
            Assert.True(second_count[Color.FromArgb(255, 0, 0, 0)] > initial_count[Color.FromArgb(255, 0, 0, 0)]);

            //TODO(tthomas): Replace SwitchTabs("Single Plot") with double click.
            //IWebElement pairs_plot = driver.FindElement(By.Id("Explore-pairs_plot"));
            //IAction dbl_click_pairs_plot = builder.MoveToElement(pairs_plot).MoveByOffset(100, 300).DoubleClick().Build();
            //dbl_click_pairs_plot.Perform();

            //Test Single Plot
            ShinyUtilities.OpenTabPanel(driver, "Explore-tabset", "Single Plot");
            var single_plot = new ShinyPlot(driver, "Explore-single_plot");
            new ShinySelectInput(driver, "Explore-x_input").SetCurrentSelectionClicked("IN_Tip_AvgCapMaterialThickness");
            single_plot.WaitUntilImageRefreshes();
            Assert.True(single_plot.ImageHasChanged());

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-single_plot_collapse", "Markers");
            new ShinySelectInput(driver, "Explore-single_plot_marker").SetCurrentSelectionClicked("16"); // "Filled Circle"
            var marker_size_single = new ShinySliderInput(driver, "Explore-single_plot_marker_size");
            Assert.Equal(1.0, marker_size_single.GetValue());
            Assert.Equal(1.5, marker_size_single.MoveSliderToValue(1.5));
            single_plot.WaitUntilImageRefreshes();
            Assert.True(single_plot.ImageHasChanged());

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-single_plot_collapse", "Filter");
            // Perform plot brush sequence
            var brush_single_plot = builder.MoveToElement(single_plot.GetElement(), 200, 200).ClickAndHold();
            brush_single_plot.MoveByOffset(400, 400).Release().Build().Perform();
            driver.FindElement(By.Id("Explore-update_y")).Click();
            builder.MoveToElement(single_plot.GetElement(), 100, 100).Click().Build().Perform();

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-single_plot_collapse", "Overlays");
            Assert.Equal("false", driver.FindElement(By.Id("Explore-add_regression")).GetAttribute("data-shinyjs-resettable-value"));
            //Assert.False(single_plot.ImageHasChanged()); // Faster Method
            Assert.False(single_plot.ImageIncludesColor(Color.FromArgb(255, 0, 0, 139)));

            driver.FindElement(By.Id("Explore-add_regression")).Click();
            single_plot.WaitUntilImageRefreshes();
            //Assert.True(single_plot.ImageHasChanged()); // Faster Method
            Assert.True(single_plot.ImageIncludesColor(Color.FromArgb(255, 0, 0, 139)));

            //Test Single Point Details
            ShinyUtilities.OpenTabPanel(driver, "Explore-tabset", "Point Details");
            new ShinySelectInput(driver, "Explore-details_guid").SetCurrentSelectionTyped("0f700");
            var expected_details = "                                               \r\nCfgID                                \"32-20\"   \r\nIN_E11                               \"27684.36\"\r\nIN_E22                               \"72611.63\"\r\nIN_ElemCount                         \"44\"      \r\nIN_HubMaterial                       \"Aluminum\"\r\nIN_Root_AvgCapMaterialThickness (mm) \"81.6862\" \r\nIN_Tip_AvgCapMaterialThickness (mm)  \"22.29602\"\r\nOUT_Blade_Cost_Total (USD)           \"148647.5\"\r\nOUT_Blade_Tip_Deflection (mm)        \"2639.237\"";
            Assert.Equal(expected_details, ShinyUtilities.ReadVerbatimText(driver, "Explore-point_details"));

            // Return to Pairs Plot
            ShinyUtilities.OpenTabPanel(driver, "Explore-tabset", "Pairs Plot");
        }

        private void TabsSet(IWebDriver driver)
        {
            var all_variable_names = "IN_E11, IN_E22, IN_ElemCount, IN_Root_AvgCapMaterialThickness, IN_Tip_AvgCapMaterialThickness, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection";
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));

            // Test "DataTable.R"
            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Data Table");
            Assert.Equal("true", driver.FindElement(By.Id("DataTable-use_filtered")).GetAttribute("data-shinyjs-resettable-value"));
            var process_method = new ShinySelectInput(driver, "DataTable-process_method");
            Assert.Equal("None", process_method.GetCurrentSelection());
            process_method.SetCurrentSelectionClicked("TOPSIS");

            var weight_metrics = new ShinySelectMultipleInput(driver, "DataTable-weightMetrics");
            //Assert.Equal(all_variable_names, string.Join(", ", weight_metrics.GetRemainingChoices().ToArray())); <-- Broken right now.
            weight_metrics.AppendSelection("OUT_Blade");
            weight_metrics.AppendSelection("OUT_Blade");


            // Test "Histogram.R"
            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Histogram");
            var histogram_variable = new ShinySelectInput(driver, "Histogram-variable");
            Assert.Equal(all_variable_names, string.Join(", ", histogram_variable.GetAllChoices().ToArray()));
            histogram_variable.SetCurrentSelectionClicked("OUT_Blade_Cost_Total");


            // Test "PETRefinement.R"
            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "PET Refinement");
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("PETRefinement-apply_original_cfg_ids")));
            ShinyUtilities.ClickIDWithScroll(driver, "PETRefinement-apply_original_cfg_ids");
            ShinyUtilities.ClickIDWithScroll(driver, "PETRefinement-apply_all_original_numeric");
            ShinyUtilities.ClickIDWithScroll(driver, "PETRefinement-apply_all_original_enum");
            ShinyUtilities.ClickIDWithScroll(driver, "PETRefinement-apply_refined_range_IN_E11");
            ShinyUtilities.ClickIDWithScroll(driver, "PETRefinement-apply_refined_range_IN_Root_AvgCapMaterialThickness");
            Assert.Equal("600", driver.FindElement(By.Id("PETRefinement-pet_num_samples")).GetAttribute("value"));
            Assert.Equal("28-16, 28-20, 30-16, 30-20, 32-16, 32-20", driver.FindElement(By.Id("PETRefinement-new_cfg_ids")).GetAttribute("value"));
            Assert.Equal("5", driver.FindElement(By.Id("PETRefinement-new_min_IN_ElemCount")).GetAttribute("value"));
            Assert.Equal("31898.59688", driver.FindElement(By.Id("PETRefinement-new_max_IN_E11")).GetAttribute("value"));
            Assert.Equal("9180", driver.FindElement(By.Id("PETRefinement-new_min_IN_E22")).GetAttribute("value"));
            Assert.Equal("77.01253438", driver.FindElement(By.Id("PETRefinement-new_min_IN_Root_AvgCapMaterialThickness")).GetAttribute("value"));
            Assert.Equal("30", driver.FindElement(By.Id("PETRefinement-new_max_IN_Tip_AvgCapMaterialThickness")).GetAttribute("value"));
            Assert.Equal("Steel, Aluminum", driver.FindElement(By.Id("PETRefinement-new_selection_IN_HubMaterial")).GetAttribute("value"));
            Assert.Equal("/Testing/Parametric Studies/WindTurbinePET_Refined", driver.FindElement(By.Id("PETRefinement-newPetName")).GetAttribute("value"));
            ShinyUtilities.ScrollToTop(driver);


            //// Test "UncertaintyQuantification.R"
            //IWait<IWebDriver> wait30 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(30.0));

            //Assert.True(wait30.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete")));
            //Assert.Equal("Visualizer", driver.Title);

            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Uncertainty Quantification");
            //driver.FindElement(By.Id("UncertaintyQuantification-design_configs_present")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.XPath("//div[@id='UncertaintyQuantification-vars_plots']/div[1]/div/div/img")).Displayed));
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/../../../..")).Displayed));
            //driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/following-sibling::div")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/following-sibling::div/div[2]")).Displayed));
            //driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-design_config_choice']/following-sibling::div/div[2]//div[@data-value='32-16']")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.XPath("//div[@id='UncertaintyQuantification-vars_plots']/div[1]/div/div/img")).Displayed));

            //// Forward UQ
            //driver.FindElement(By.Id("UncertaintyQuantification-fuq_constraint_enable2")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.Id("UncertaintyQuantification-fuq_constraint_enable2")).Selected));
            //driver.FindElement(By.Id("UncertaintyQuantification-run_forward_uq")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.XPath("//div[@id='UncertaintyQuantification-vars_plots']/div[1]/div/div/img")).Displayed));

            ////// Add Probability Query
            //driver.FindElement(By.Id("UncertaintyQuantification-add_probability")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.Id("UncertaintyQuantification-queryThreshold0")).Displayed));

            //driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-queryVariable0']/following-sibling::div")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-queryVariable0']/following-sibling::div/div[2]")).Displayed));
            //driver.FindElement(By.XPath("//*[@id='UncertaintyQuantification-queryVariable0']/following-sibling::div/div[2]//div[@data-value='OUT_Blade_Tip_Deflection']")).Click();



            ////driver.FindElement(By.XPath("//select[@data-value='OUT_Blade_Tip_Deflection']")).Click();

            //driver.FindElement(By.Id("UncertaintyQuantification-queryThreshold0")).SendKeys("2400");
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.CssSelector("#UncertaintyQuantification-queryThreshold0")).GetAttribute("value") == "2400"));

            ////// Evaluate current probability Query
            //driver.FindElement(By.Id("UncertaintyQuantification-run_probabilities_queries")).Click();
            //Assert.True(wait30.Until(driver1 => driver.FindElement(By.Id("UncertaintyQuantification-queryValue0")).Displayed));
            //Assert.True(wait30.Until(driver1 => float.Parse(driver.FindElement(By.Id("UncertaintyQuantification-queryValue0")).Text) < 0.35));

            /////*      DESIGN RANKING TAB      */
            ////driver.FindElement(By.CssSelector("#uqTabset > li:nth-child(2) > a")).Click();

            ////IWait<IWebDriver> UQ_wait4 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            ////Assert.True(UQ_wait4.Until(driver1 => driver.FindElement(By.CssSelector("#runProbability")).Displayed));
            ////driver.FindElement(By.CssSelector("#runProbability")).Click();

            /////*
            ////IWait<IWebDriver> UQ_wait5 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            ////Assert.True(UQ_wait5.Until(driver1 => driver.FindElement(By.CssSelector("#probabilityTable")).Displayed));
            ////*/

            // Return to "Explore.R" tab
            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Explore");
        }

        private void FooterSet(IWebDriver driver)
        {
            // TODO(tthomas): Add testing of additional UI elements
            
            // Test Footer

            IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            Assert.True(wait10.Until(driver1 => driver.FindElement(By.XPath("//*[@id='Explore-pairs_plot']/img")).Displayed));

            // Filters
            IWebElement elemcountslider = driver.FindElement(By.Id("filter_IN_ElemCount")).FindElement(By.XPath(".."));
            if (!elemcountslider.Displayed)
            {
                RetryStaleElement(() => driver.FindElement(By.LinkText("Filters")).Click());
                wait10.Until(driver1 => elemcountslider.Displayed);
            }
            ShinyUtilities.ScrollToElement(driver, elemcountslider);
            Actions dblclick_elemcountslider = new Actions(driver).DoubleClick(elemcountslider);
            RetryStaleElement(() => dblclick_elemcountslider.Build().Perform());
            
            wait10.Until(driver1 => driver.FindElement(By.Id("tooltip_min_IN_ElemCount")).Displayed);
            driver.FindElement(By.Id("tooltip_min_IN_ElemCount")).Clear();
            driver.FindElement(By.Id("tooltip_min_IN_ElemCount")).SendKeys("20");
            driver.FindElement(By.Id("submit_IN_ElemCount")).Click();
            Thread.Sleep(1500);

            IWebElement costslider = driver.FindElement(By.Id("filter_OUT_Blade_Cost_Total")).FindElement(By.XPath(".."));
            wait10.Until(driver1 => costslider.Displayed);

            string jsToBeExecuted = string.Format("window.scroll(0, {0});", costslider.Location.Y);
            ((IJavaScriptExecutor)driver).ExecuteScript(jsToBeExecuted);
                
            Actions dblclick_costslider = new Actions(driver).DoubleClick(costslider);
            RetryStaleElement(() => dblclick_costslider.Build().Perform());

            wait10.Until(driver1 => driver.FindElement(By.Id("tooltip_min_OUT_Blade_Cost_Total")).Displayed);
            driver.FindElement(By.Id("tooltip_min_OUT_Blade_Cost_Total")).Clear();
            driver.FindElement(By.Id("tooltip_min_OUT_Blade_Cost_Total")).SendKeys("150000");
            driver.FindElement(By.Id("submit_OUT_Blade_Cost_Total")).Click();
            Thread.Sleep(1500);

            // Coloring
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Coloring");
            var coloring_source = new ShinySelectInput(driver, "coloring_source");
            Assert.Equal("None", coloring_source.GetCurrentSelection());
            Assert.Equal(2, coloring_source.GetAllChoices().Count());
            Assert.Equal("None, Live", string.Join(", ", coloring_source.GetAllChoices().ToArray()));
            coloring_source.SetCurrentSelectionClicked("Live");
            var colored_variable = new ShinySelectInput(driver, "live_coloring_variable_numeric");
            var choices = colored_variable.GetAllChoices();
            Assert.Equal("IN_E11, IN_E22, IN_ElemCount, IN_Root_AvgCapMaterialThickness, IN_Tip_AvgCapMaterialThickness, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection", string.Join(", ", colored_variable.GetAllChoices().ToArray()));
            colored_variable.SetCurrentSelectionClicked("OUT_Blade_Cost_Total");
            driver.FindElement(By.Id("live_coloring_name")).Clear();
            driver.FindElement(By.Id("live_coloring_name")).SendKeys("Test");
            driver.FindElement(By.Id("live_coloring_add_classification")).Click();
            Assert.True(wait10.Until(d => coloring_source.GetAllChoices().Where(c => c == "Test").Count() == 1));

            // Classifications
            //driver.FindElement(By.LinkText("Classifications")).Click();
            //wait10.Until(driver1 => driver.FindElement(By.Id("no_classifications")).Displayed);
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Classifications");
            Assert.Equal("No Classifications Available.", driver.FindElement(By.Id("no_classifications")).Text);

            // Configuration
            driver.FindElement(By.LinkText("Configuration")).Click();
            wait10.Until(driver1 => driver.FindElement(By.Id("remove_missing")).Displayed);
            Assert.Equal("false", driver.FindElement(By.Id("remove_missing")).GetAttribute("data-shinyjs-resettable-value"));
            Assert.Equal("false", driver.FindElement(By.Id("remove_outliers")).GetAttribute("data-shinyjs-resettable-value"));
        }

        private void TabsCheck(IWebDriver driver)
        {
            IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            Assert.True(wait10.Until(driver1 => driver.FindElement(By.XPath("//*[@id='Explore-pairs_plot']/img")).Displayed));
            var display = new ShinySelectMultipleInput(driver, "Explore-display");
            Assert.Equal("IN_HubMaterial, IN_E11, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection", string.Join(", ", display.GetCurrentSelection().ToArray()));

            // Test Data Table
            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Data Table");
            var weight_metrics = new ShinySelectMultipleInput(driver, "DataTable-weightMetrics");
            Assert.Equal("OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection", string.Join(", ", weight_metrics.GetCurrentSelection().ToArray()));
            wait10.Until(ExpectedConditions.ElementIsVisible(By.Id("DataTable-clearMetrics"))).Click();
            Thread.Sleep(500); // FIXME: Apply the correct wait statement here instead of a Thread.Sleep() call.
            Assert.Equal("", string.Join(", ", weight_metrics.GetCurrentSelection().ToArray()));
        }

        class DigWrapper : IDisposable
        {
            public StringBuilder stdoutData = new StringBuilder();
            public StringBuilder stderrData = new StringBuilder();
            public string url;
            Process proc;

            public void Start(string input_filename, bool from_csv = false)
            {
                Process proc = new Process();
                proc.StartInfo.Arguments = "--no-save --no-restore -e \"shiny::runApp('Dig',display.mode='normal',quiet=FALSE, launch.browser=FALSE)\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = Path.Combine(META.VersionInfo.MetaPath, @"bin\R\bin\x64\Rscript.exe");
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.WorkingDirectory = Path.Combine(META.VersionInfo.MetaPath, @"bin");
                if (from_csv)
                {
                    proc.StartInfo.EnvironmentVariables["DIG_INPUT_CSV"] = input_filename;
                }
                else
                {
                    proc.StartInfo.EnvironmentVariables["DIG_DATASET_CONFIG"] = input_filename;
                }
                
                ManualResetEvent task = new ManualResetEvent(false);
                using (task)
                {
                    proc.OutputDataReceived += (o, args) =>
                    {
                        if (args.Data != null)
                        {
                            lock (stdoutData)
                            {
                                stdoutData.Append(args.Data + Environment.NewLine);
                            }
                        }
                    };
                    proc.ErrorDataReceived += (o, args) =>
                    {
                        if (args.Data != null)
                        {
                            lock (stderrData)
                            {
                                stderrData.Append(args.Data + Environment.NewLine);
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

            public void AppendLog(string log)
            {
                File.AppendAllText(log, stdoutData.ToString());
            }
        }
    }
}
