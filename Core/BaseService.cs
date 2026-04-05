using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace WeBook.Core
{
    public abstract class BaseService
    {
        // Every service can now use these shared tools
        protected static WebDriverWait GetWait(IWebDriver driver, int seconds = 10)
            => new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

        protected static IJavaScriptExecutor GetJs(IWebDriver driver)
            => (IJavaScriptExecutor)driver;
    }
}
