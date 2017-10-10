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
                DataTableSet(driver);
                TabsSet(driver);
                FooterSet(driver);

                Thread.Sleep(500); //For shiny to catch up, find a better way
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
                Assert.True(ShinyUtilities.WaitUntilDocumentReady(driver));

                ExploreCheck(driver);
                DataTableCheck(driver);
                FooterCheck(driver);

                driver.Close();
                wrapper.AppendLog(session.log_file);
            }

            File.Delete(session.copied_config);
            File.Delete(session.data_file);
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
            Assert.True(autorender.GetStartState());
            //TODO(tthomas): Test Delayed Render
            //Assert.False(autorender.ToggleState());
            //ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Variables");

            var upperpanel = new ShinyCheckboxInput(driver, "Explore-pairs_upper_panel");
            Assert.False(upperpanel.GetStartState());
            Assert.True(upperpanel.ToggleState());
            pairs_plot.WaitUntilImageRefreshes();
            Assert.True(pairs_plot.ImageStats()[Color.FromArgb(255,0,0,0)] > start[Color.FromArgb(255,0,0,0)] * 1.5);

            var trendlines = new ShinyCheckboxInput(driver, "Explore-pairs_trendlines");
            Assert.False(trendlines.GetStartState());
            Assert.False(start.ContainsKey(Color.FromArgb(255, 255, 0, 0)));
            Assert.True(trendlines.ToggleState());
            pairs_plot.WaitUntilImageRefreshes();
            Assert.True(pairs_plot.ImageIncludesColor(Color.FromArgb(255, 255, 0, 0)));

            var displayunits = new ShinyCheckboxInput(driver, "Explore-pairs_units");
            Assert.True(displayunits.GetStartState());

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Markers");
            var initial_count = pairs_plot.ImageStats();
            new ShinySelectInput(driver, "Explore-pairs_plot_marker").SetCurrentSelectionTyped("Plus");
            pairs_plot.WaitUntilImageRefreshes();
            var second_count = pairs_plot.ImageStats();
            Assert.True(second_count[Color.FromArgb(255, 0, 0, 0)] > initial_count[Color.FromArgb(255, 0, 0, 0)]);
            Assert.Equal(1.5, new ShinySliderInput(driver, "Explore-pairs_plot_marker_size").MoveSliderToValue(1.5));
            pairs_plot.WaitUntilImageRefreshes();
            var third_count = pairs_plot.ImageStats();
            Assert.True(third_count[Color.FromArgb(255, 0, 0, 0)] > second_count[Color.FromArgb(255, 0, 0, 0)]);
            
            //TODO(tthomas): Replace OpenTabPanel("Single Plot") with double click.
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
            var in_e11_filter = new VisualizerFilterInput(driver, "IN_E11");
            Assert.Equal("25520-32480", in_e11_filter.GetFromTo());
            driver.FindElement(By.Id("Explore-update_y")).Click();
            builder.MoveToElement(single_plot.GetElement(), 100, 100).Click().Build().Perform();
            Assert.Equal("25520-32480", in_e11_filter.GetFromTo()); // This should fail when UpdateX/Y/Both are fixed.

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

        private void ExploreCheck(IWebDriver driver)
        {
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            
            // Test Pairs Plot
            Assert.True(wait.Until(driver1 => driver.FindElement(By.XPath("//*[@id='Explore-pairs_plot']/img")).Displayed));
            var display = new ShinySelectMultipleInput(driver, "Explore-display");
            Assert.Equal("IN_HubMaterial, IN_E11, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection", string.Join(", ", display.GetCurrentSelection().ToArray()));

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Plot Options");
            Assert.True(new ShinyCheckboxInput(driver, "Explore-auto_render").GetStartState());
            Assert.True(new ShinyCheckboxInput(driver, "Explore-pairs_upper_panel").GetStartState());
            Assert.True(new ShinyCheckboxInput(driver, "Explore-pairs_trendlines").GetStartState());
            Assert.True(new ShinyCheckboxInput(driver, "Explore-pairs_units").GetStartState());

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-pairs_plot_collapse", "Markers");
            Assert.Equal("3", new ShinySelectInput(driver, "Explore-pairs_plot_marker").GetCurrentSelection());
            Assert.Equal(1.5, new ShinySliderInput(driver, "Explore-pairs_plot_marker_size").GetValue());
            
            //Test Single Plot
            ShinyUtilities.OpenTabPanel(driver, "Explore-tabset", "Single Plot");
            Assert.Equal("IN_Tip_AvgCapMaterialThickness", new ShinySelectInput(driver, "Explore-x_input").GetCurrentSelection());
            Assert.Equal("IN_E11", new ShinySelectInput(driver, "Explore-y_input").GetCurrentSelection());
            
            ShinyUtilities.OpenCollapsePanel(driver, "Explore-single_plot_collapse", "Markers");
            Assert.Equal("16", new ShinySelectInput(driver, "Explore-single_plot_marker").GetCurrentSelection()); // "Filled Circle"
            Assert.Equal(1.5, new ShinySliderInput(driver, "Explore-single_plot_marker_size").GetValue());

            ShinyUtilities.OpenCollapsePanel(driver, "Explore-single_plot_collapse", "Overlays");
            Assert.True(new ShinyCheckboxInput(driver, "Explore-add_regression").GetStartState());
            Assert.False(new ShinyCheckboxInput(driver, "Explore-add_contour").GetStartState());
            Assert.False(new ShinyCheckboxInput(driver, "Explore-add_pareto").GetStartState());

            //Test Single Point Details
            ShinyUtilities.OpenTabPanel(driver, "Explore-tabset", "Point Details");
            Assert.True(new ShinySelectInput(driver, "Explore-details_guid").GetCurrentSelection().StartsWith("0f700"));
            var expected_details = "                                               \r\nCfgID                                \"32-20\"   \r\nIN_E11                               \"27684.36\"\r\nIN_E22                               \"72611.63\"\r\nIN_ElemCount                         \"44\"      \r\nIN_HubMaterial                       \"Aluminum\"\r\nIN_Root_AvgCapMaterialThickness (mm) \"81.6862\" \r\nIN_Tip_AvgCapMaterialThickness (mm)  \"22.29602\"\r\nOUT_Blade_Cost_Total (USD)           \"148647.5\"\r\nOUT_Blade_Tip_Deflection (mm)        \"2639.237\"";
            Assert.Equal(expected_details, ShinyUtilities.ReadVerbatimText(driver, "Explore-point_details"));

            // Return to Pairs Plot
            ShinyUtilities.OpenTabPanel(driver, "Explore-tabset", "Pairs Plot");
        }

        private void DataTableSet(IWebDriver driver)
        {
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));

            // Test "DataTable.R"
            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Data Table");
            Assert.True(new ShinyCheckboxInput(driver, "DataTable-use_filtered").GetStartState());
            var process_method = new ShinySelectInput(driver, "DataTable-process_method");
            Assert.Equal("None", process_method.GetCurrentSelection());
            process_method.SetCurrentSelectionClicked("TOPSIS");

            var weight_metrics = new ShinySelectMultipleInput(driver, "DataTable-weightMetrics");
            //TODO(tthomas): Fix the ShinySelectMultipleInput.GetRemainingChoices() call.
            //var all_numeric_variable_names = "IN_E11, IN_E22, IN_ElemCount, IN_Root_AvgCapMaterialThickness, IN_Tip_AvgCapMaterialThickness, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection";
            //var sample = string.Join(", ", weight_metrics.GetRemainingChoices().ToArray());
            //wait.Until(d => string.Join(", ", weight_metrics.GetRemainingChoices().ToArray()) == all_numeric_variable_names);
            weight_metrics.AppendSelection("OUT_Blade");
            weight_metrics.AppendSelection("OUT_Blade");
            Thread.Sleep(200);

            Assert.Equal(0.5, new ShinySliderInput(driver, "DataTable-rnk7").MoveSliderToValue(0.5));
            wait.Until(d => driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[1]")).GetAttribute("textContent") == "140");
            Assert.Equal("1", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[2]")).GetAttribute("textContent"));
            Assert.Equal("0.828514676583442", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[3]")).GetAttribute("textContent"));
            Assert.Equal("32-16", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[4]")).GetAttribute("textContent"));
            Assert.Equal("8a3c95db-2fa6-4fbc-badd-34a119d1c37e", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[5]")).GetAttribute("textContent"));
        }

        /// <summary>
        /// Check Data Table tab after session restore.
        /// </summary>
        /// <param name="driver"></param>
        private void DataTableCheck(IWebDriver driver)
        {
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));

            ShinyUtilities.OpenTabPanel(driver, "master_tabset", "Data Table");
            Assert.True(new ShinyCheckboxInput(driver, "DataTable-use_filtered").GetStartState());
            Assert.Equal("TOPSIS", new ShinySelectInput(driver, "DataTable-process_method").GetCurrentSelection());

            //TODO(tthomas): Fix failure to restore weight slider value.
            //Assert.Equal(0.5, new ShinySliderInput(driver, "DataTable-rnk7").GetValue());
            //wait.Until(d => driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[1]")).GetAttribute("textContent") == "140");
            //Assert.Equal("1", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[2]")).GetAttribute("textContent"));
            //Assert.Equal("0.828514676583442", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[3]")).GetAttribute("textContent"));
            //Assert.Equal("32-16", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[4]")).GetAttribute("textContent"));
            //Assert.Equal("8a3c95db-2fa6-4fbc-badd-34a119d1c37e", driver.FindElement(By.XPath("//div[@id='DataTable-dataTable']/div[1]/table/tbody/tr[1]/td[5]")).GetAttribute("textContent"));

            var weight_metrics = new ShinySelectMultipleInput(driver, "DataTable-weightMetrics");
            Assert.Equal("OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection", string.Join(", ", weight_metrics.GetCurrentSelection().ToArray()));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("DataTable-clearMetrics"))).Click();
            Thread.Sleep(500); // FIXME: Apply the correct wait statement here instead of a Thread.Sleep() call.
            wait.Until(d => string.Join(", ", weight_metrics.GetCurrentSelection().ToArray()) == "");
        }

        private void TabsSet(IWebDriver driver)
        {
            var all_variable_names = "IN_E11, IN_E22, IN_ElemCount, IN_Root_AvgCapMaterialThickness, IN_Tip_AvgCapMaterialThickness, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection";
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));

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

        /// <summary>
        /// Check the Footer after a session restore.
        /// </summary>
        private void FooterSet(IWebDriver driver)
        {
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));

            // Filters
            ShinyUtilities.ScrollToElementID(driver, "footer_collapse");
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Filters");
            Assert.Equal("20-50", new VisualizerFilterInput(driver, "IN_ElemCount").EntrySetFromTo(20, 50));
            Assert.Equal(20000, new VisualizerFilterInput(driver, "IN_E22").EntrySetFrom(20000.0));
            Assert.Equal(160000, new VisualizerFilterInput(driver, "OUT_Blade_Cost_Total").EntrySetTo(160000));

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
            Assert.True(wait.Until(d => coloring_source.GetAllChoices().Where(c => c == "Test").Count() == 1));

            // Classifications
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Classifications");
            Assert.Equal("No Classifications Available.", driver.FindElement(By.Id("no_classifications")).Text);

            // Configuration
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Configuration");
            var remove_missing = new ShinyCheckboxInput(driver, "remove_missing");
            Assert.False(remove_missing.GetStartState());
            var remove_outliers = new ShinyCheckboxInput(driver, "remove_outliers");
            Assert.False(remove_outliers.GetStartState());
            var stats = new VisualizerFilterStats(driver);
            var prev_points = stats.GetCurrentPoints();
            remove_outliers.ToggleState();
            new ShinySliderInput(driver, "num_sd").MoveSliderToValue(2);

            // Return to Filters
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Filters");
            Assert.True(stats.GetCurrentPoints() < prev_points);
        }

        private void FooterCheck(IWebDriver driver)
        {
            IWait<IWebDriver> wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));

            // Filters
            ShinyUtilities.ScrollToElementID(driver, "footer_collapse");
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Filters");
            Assert.Equal("20-50", new VisualizerFilterInput(driver, "IN_ElemCount").GetFromTo());
            Assert.Equal(20000, new VisualizerFilterInput(driver, "IN_E22").GetFrom());
            Assert.Equal(160000, new VisualizerFilterInput(driver, "OUT_Blade_Cost_Total").GetTo());

            // Coloring
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Coloring");
            var coloring_source = new ShinySelectInput(driver, "coloring_source");
            Assert.Equal("Live", coloring_source.GetCurrentSelection());
            Assert.Equal("None, Live, Test", string.Join(", ", coloring_source.GetAllChoices().ToArray()));
            var colored_variable = new ShinySelectInput(driver, "live_coloring_variable_numeric");
            var choices = colored_variable.GetAllChoices();
            Assert.Equal("IN_E11, IN_E22, IN_ElemCount, IN_Root_AvgCapMaterialThickness, IN_Tip_AvgCapMaterialThickness, OUT_Blade_Cost_Total, OUT_Blade_Tip_Deflection", string.Join(", ", colored_variable.GetAllChoices().ToArray()));
            Assert.Equal("OUT_Blade_Cost_Total", colored_variable.GetCurrentSelection());

            // Classifications
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Classifications");
            Assert.Equal("No Classifications Available.", driver.FindElement(By.Id("no_classifications")).Text);

            // Configuration
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Configuration");
            var remove_missing = new ShinyCheckboxInput(driver, "remove_missing");
            Assert.False(remove_missing.GetStartState());
            var remove_outliers = new ShinyCheckboxInput(driver, "remove_outliers");
            Assert.True(remove_outliers.GetStartState());
            Assert.Equal(2, new ShinySliderInput(driver, "num_sd").GetValue());

            // Return to Filters
            ShinyUtilities.OpenCollapsePanel(driver, "footer_collapse", "Filters");
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
