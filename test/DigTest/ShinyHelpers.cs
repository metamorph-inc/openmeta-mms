using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DigTest
{
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
        public static void SwitchTabs(IWebDriver driver, string name)
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5.0));
            driver.FindElement(By.LinkText(name)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//li[contains(.,'" + name + "')]/../../div/div[@class='tab-pane active' and @data-value='" + name + "']")));
        }
    }


}
