using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace DigTest
{
    public class ShinySession
    {
        public string all_vars { get; set; }
        public string original_config { get; set; }
        public string copied_config { get; set; }
        public string log_file { get; set; }

        public ShinySession(string config_file)
        {
            all_vars = "";
            original_config = Path.Combine(META.VersionInfo.MetaPath, config_file);
            copied_config = original_config.Insert(original_config.LastIndexOf(".json"), "_test");
            log_file = Path.ChangeExtension(copied_config, ".log");
        }
    }

    public class ShinySliderInput
    {
        public string id;
        private IWebDriver driver;
        private IWait<IWebDriver> wait;
        private string grid_path;
        private string current_path;
        private double low;
        private double high;
        private double current { get; set; }

        public ShinySliderInput(IWebDriver driver, string id)
        {
            this.driver = driver;
            this.id = id;
            this.wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(1.0));
            string parent_path = String.Format("//input[@id='{0}']/..", id);
            this.grid_path = parent_path + "/span/span[@class='irs-grid']";
            string low_path = parent_path + "/span/span[@class='irs']/span[@class='irs-min']";
            this.current_path = parent_path + "/span/span[@class='irs']/span[@class='irs-single']";
            string high_path = parent_path + "/span/span[@class='irs']/span[@class='irs-max']";
            this.low = Double.Parse(driver.FindElement(By.XPath(low_path)).GetAttribute("textContent"));
            this.current = Double.Parse(driver.FindElement(By.XPath(current_path)).GetAttribute("textContent"));
            this.high = Double.Parse(driver.FindElement(By.XPath(high_path)).GetAttribute("textContent"));
        }

        public double MoveSliderToValue(double target)
        {
            if(target < low) { target = low; }
            if(target > high) { target = high; }
            Actions builder = new Actions(this.driver);
            var grid = driver.FindElement(By.XPath(this.grid_path));
            var width = grid.Size.Width;
            var old_x = width * (current - low) / (high - low);
            var new_x = width * (target - low) / (high - low);
            builder.MoveToElement(grid, (int)old_x, 0).ClickAndHold();
            builder.MoveByOffset((int)(new_x - old_x), 0).Release().Build().Perform();
            this.current = Double.Parse(driver.FindElement(By.XPath(current_path)).GetAttribute("textContent"));
            return this.current;
        }
    }

    public class ShinySelectInput
    {
        private string id;
        private IWebDriver driver;
        private IWait<IWebDriver> wait;
        private string div;
        private string choices;
        private string selected;

        public ShinySelectInput(IWebDriver driver, string id)
        {
            this.driver = driver;
            this.id = id;
            this.div = string.Format("//select[@id='{0}']/..", id);
            var master_div = this.driver.FindElement(By.XPath(this.div));
            this.choices = this.div + "/select/following::div[1]/div[2]/div[1]/div";
            this.selected = choices + "[@class='option selected']";
            this.wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(11.0));
            try
            {
                this.driver.FindElement(By.XPath(this.choices));
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                // Force the choices to appear.
                master_div.Click();
                master_div.Click();
            }
            if (this.driver.FindElement(By.XPath(this.choices)).GetAttribute("data-value") == null)
            {
                var new_base = string.Format("//select[@id='{0}']/following::div[1]/div[2]/div[1]/div/div", id);
                this.choices = new_base + "[@class='option selected' or @class='option']";
                this.selected = new_base + "[@class='option selected']";
            }
        }

        public string GetCurrentSelection()
        {
            this.wait.Until(ExpectedConditions.ElementExists(By.XPath(this.selected)));
            return this.driver.FindElement(By.XPath(this.selected)).GetAttribute("data-value");
        }

        public IWebElement GetDiv()
        {
            return this.driver.FindElement(By.XPath(this.div));
        }

        private IEnumerable<IWebElement> GetAllChoiceDivs()
        {
            return this.driver.FindElements(By.XPath(this.choices));
        }

        public IEnumerable<string> GetAllChoices()
        {
            // TODO(tthomas): Find a way to turn the retry logic into a function.
            IEnumerable<string> choices;
            int tries = 3;
            while (true)
            {
                try
                {
                    var choices_divs = this.driver.FindElements(By.XPath(this.choices));
                    var choices_list = new List<string>();
                    for(var i = 0; i < choices_divs.Count(); i++)
                    {
                        choices_list.Add(choices_divs[i].GetAttribute("data-value"));
                    }
                    choices = choices_list.AsEnumerable();
                    break;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    Thread.Sleep(300);
                    if (--tries == 0)
                    {
                        throw;
                    }
                }
                catch (OpenQA.Selenium.StaleElementReferenceException)
                {
                    Thread.Sleep(300);
                    if (--tries == 0)
                    {
                        throw;
                    }
                }
            }
            return choices;
        }

        public void SetCurrentSelection(string v)
        {
            var master_div = this.driver.FindElement(By.XPath(this.div));
            master_div.Click();
            Thread.Sleep(300);
            var choices = this.GetAllChoiceDivs();
            var to_select = from choice in choices
                            where choice.GetAttribute("data-value") == v
                            select choice;
            this.wait.Until(driver1 => to_select.First().Displayed);
            to_select.First().Click();
        }
    }

    public class ShinySelectMultipleInput
    {
        private IWebDriver driver;
        private string id;
        private string div;
        private string input;
        private string choices;
        private string selected;
        private WebDriverWait wait;

        public ShinySelectMultipleInput(IWebDriver driver, string id)
        {
            this.driver = driver;
            this.id = id;
            this.div = string.Format("//select[@id='{0}']/..", id);
            var master_div = this.driver.FindElement(By.XPath(this.div));
            this.input = this.div + "/div[1]/div[1]/input";
            this.choices = this.div + "/div[1]/div[2]/div/div";
            this.selected = this.div + "/div[1]/div[1]/div[@class='item']";
            this.wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(11.0));
            try
            {
                this.driver.FindElement(By.XPath(this.choices));
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                // Force the choices to appear.
                Thread.Sleep(2000);
                master_div.Click();
                master_div.Click();
            }
            if (this.driver.FindElement(By.XPath(this.choices)).GetAttribute("data-value") == null)
            {
                this.choices = string.Format("//select[@id='{0}']/following::div[1]/div[2]/div[1]/div/div[@class='option selected' or @class='option']", id);
                this.selected = string.Format("//select[@id='{0}']/following::div[1]/div[1]/div[@class='item']", id);
            }
        }

        public IEnumerable<string> GetCurrentSelection()
        {
            IEnumerable<string> selected = null;
            try
            {
                selected = from selected_div in this.driver.FindElements(By.XPath(this.selected))
                           select selected_div.GetAttribute("data-value");
                return selected;
            }
            catch (OpenQA.Selenium.NoSuchElementException)
            {
                return null;
            }
        }

        public IWebElement GetDiv()
        {
            return this.driver.FindElement(By.XPath(this.div));
        }

        private IEnumerable<IWebElement> GetAllChoiceDivs()
        {
            return this.driver.FindElements(By.XPath(this.choices));
        }

        public IEnumerable<string> GetRemainingChoices()
        {
            // TODO(tthomas): Find a way to turn the retry logic into a function.
            IEnumerable<string> choices;
            Thread.Sleep(3000);
            int tries = 3;
            while (true)
            {
                try
                {
                    choices = from choice_div in this.driver.FindElements(By.XPath(this.choices))
                              select choice_div.GetAttribute("data-value");
                    break;
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    Thread.Sleep(300);
                    if (--tries == 0)
                    {
                        throw;
                    }
                }
            }
            return choices;
        }

        public void AppendSelection(string v)
        {
            var input = this.driver.FindElement(By.XPath(this.input));
            input.SendKeys(Keys.ArrowRight);
            input.SendKeys(v);
            input.SendKeys(Keys.Enter);
            input.SendKeys(Keys.Escape);
        }
    }

    public class ShinyUtilities
    {
        public static void OpenTabPanel(IWebDriver driver, string tabset_id, string tab_name)
        {
            string link_path = String.Format("//ul[@id='{0}']/li/a[@data-value='{1}']",
                                             tabset_id, tab_name);
            string content_path = String.Format("//ul[@id='{0}']/../div/div[@data-value='{1}']",
                                                tabset_id, tab_name);
            if (!driver.FindElement(By.XPath(content_path)).GetAttribute("class").Split().Contains("active"))
            {
                if (!driver.FindElement(By.XPath(link_path)).Displayed)
                {
                    ScrollToElement(driver, driver.FindElement(By.XPath(link_path)));
                }
                driver.FindElement(By.XPath(link_path)).Click();
                WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                IWebElement content = driver.FindElement(By.XPath(content_path));
                wait.Until(d => content.GetAttribute("class").Split().Contains("active"));
            }
        }

        public static void OpenCollapsePanel(IWebDriver driver, string collapse_id, string panel_name)
        {
            string base_path = String.Format("//div[@id='{0}']/div[@value='{1}']",
                                             collapse_id, panel_name);
            string link_path = base_path + "/div[@class='panel-heading']/h4/a";
            string content_path = base_path + "/div[2]";
            if(!driver.FindElement(By.XPath(content_path)).GetAttribute("class").Split().Contains("in"))
            {
                if (!driver.FindElement(By.XPath(link_path)).Displayed)
                {
                    ScrollToElement(driver, driver.FindElement(By.XPath(link_path)));
                }
                driver.FindElement(By.XPath(link_path)).Click();
                WebDriverWait wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
                IWebElement content = driver.FindElement(By.XPath(content_path));
                wait.Until(d => content.GetAttribute("class").Split().Contains("in"));
                wait.Until(d => content.GetAttribute("style") == "");
            }
        }

        public static void ScrollToElement(IWebDriver driver, IWebElement elem)
        {
            string jsToBeExecuted = string.Format("window.scroll(0, {0});", elem.Location.Y);
            ((IJavaScriptExecutor)driver).ExecuteScript(jsToBeExecuted);
        }

        public static void ClickIDWithScroll(IWebDriver driver, string id)
        {
            IWebElement elem = driver.FindElement(By.Id(id));
            try
            {
                elem.Click();
            }
            catch (System.InvalidOperationException)
            {
                ScrollToElement(driver, elem);
                elem.Click();
            }
        }

        public static void ScrollToTop(IWebDriver driver)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scroll(0, 0);");
        }

        public static bool WaitUntilDocumentReady(IWebDriver driver)
        {
            IWait<IWebDriver> wait10 = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10.0));
            return wait10.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        internal static void WaitUntilImageRefreshes(IWebDriver driver, string image_id)
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromMilliseconds(200.0));
            wait.Until(d => driver.FindElement(By.Id(image_id)).GetAttribute("class").Contains("recalculating"));
            wait.Until(d => !driver.FindElement(By.Id(image_id)).GetAttribute("class").Contains("recalculating"));
        }

        // Return the number of matching pixels.
        private static int CountPixels(Bitmap bm, Color target_color)
        {
            // Loop through the pixels.
            int matches = 0;
            for (int y = 0; y < bm.Height; y++)
            {
                for (int x = 0; x < bm.Width; x++)
                {
                    if (bm.GetPixel(x, y) == target_color) matches++;
                }
            }
            return matches;
        }

        // Search for specific color.
        private static bool FindColor(Bitmap bm, Color target_color)
        {
            // Loop through the pixels.
            for (int y = 0; y < bm.Height; y++)
            {
                for (int x = 0; x < bm.Width; x++)
                {
                    if (bm.GetPixel(x, y) == target_color) return true;
                }
            }
            return false;
        }

        private static byte [] GetData(string src)
        {
            string trim = src.Substring("data:image/png;base64,".Length);
            return Convert.FromBase64String(trim);
        }

        public static bool ImageIncludesColor(IWebDriver driver, string id, Color color)
        {
            string img_xpath = String.Format("//div[@id='{0}']/img", id);
            IWebElement img_elem = driver.FindElement(By.XPath(img_xpath));
            string img_str = img_elem.GetAttribute("src");
            Bitmap image = null;
            using (MemoryStream stream = new MemoryStream(GetData(img_str)))
            {
                image = new Bitmap(stream);
            }
            return FindColor(image, color);
        }

        public static int ImageColorCount(IWebDriver driver, string id, Color color)
        {
            string img_xpath = String.Format("//div[@id='{0}']/img", id);
            IWebElement img_elem = driver.FindElement(By.XPath(img_xpath));
            string img_str = img_elem.GetAttribute("src");
            Bitmap image = null;
            using (MemoryStream stream = new MemoryStream(GetData(img_str)))
            {
                image = new Bitmap(stream);
            }
            return CountPixels(image, color);
        }
    }
}
