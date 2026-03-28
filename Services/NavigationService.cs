using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using WeBook.Helpers;

namespace WeBook.Services
{
    public class NavigationService
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public NavigationService(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        public bool HandleCookies()
        {
            try
            {
                var btn = _wait.Until(d => d.FindElements(By.XPath("//button[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'accept')]"))
                    .FirstOrDefault(e => e.Displayed));

                if (btn != null)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
                    Logger.Success("Cookies accepted");
                    System.Threading.Thread.Sleep(1000);
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}