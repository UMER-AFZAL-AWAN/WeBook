using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace WeBook.Core
{
    public static class Navigator
    {
        public static void GoToEvent(IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
            // Robust Cookie Removal
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
            var btn = document.querySelector('button#onetrust-reject-all-handler, .onetrust-close-btn-handler');
            if(btn) btn.click();
            var sdk = document.getElementById('onetrust-banner-sdk');
            if(sdk) sdk.remove();
        ");
        }
    }
}
