using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace WeBook.Core
{
    public static class DriverFactory
    {
        public static IWebDriver Create()
        {
            foreach (var p in Process.GetProcessesByName("chromedriver")) { try { p.Kill(); } catch { } }
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            return new ChromeDriver(options);
        }
    }
}
