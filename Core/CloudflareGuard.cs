using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using WeBook.Utilities;

namespace WeBook.Core
{
    public static class CloudflareGuard
    {
        public static void Check(IWebDriver driver)
        {
            try
            {
                // Short wait to not block too long
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(2));

                // Look for Cloudflare iframe or Turnstile wrapper
                var cfFrame = driver.FindElements(By.XPath("//iframe[contains(@src, 'cloudflare')]")).FirstOrDefault();
                if (cfFrame != null)
                {
                    Logger.Log("⚠️ Cloudflare iframe detected. Attempting to solve...");
                    driver.SwitchTo().Frame(cfFrame);
                    var checkbox = driver.FindElements(By.CssSelector("input[type='checkbox'], #challenge-stage")).FirstOrDefault();
                    checkbox?.Click();
                    driver.SwitchTo().DefaultContent();
                    Thread.Sleep(3000);
                    Logger.Log("✅ Cloudflare checkbox clicked.");
                    return;
                }

                // Check for Turnstile text challenge
                if (driver.PageSource.Contains("Quick security check") || driver.PageSource.Contains("Verify you are human"))
                {
                    Logger.Log("⚠️ Security check page detected. Waiting 8 seconds for auto-resolve...");
                    Thread.Sleep(8000);
                }

                // Also look for any visible Turnstile widget
                var turnstile = driver.FindElements(By.CssSelector(".cf-turnstile, #turnstile-wrapper, iframe[src*='turnstile']"));
                if (turnstile.Any())
                {
                    Logger.Log("⚠️ Turnstile widget present. Waiting 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
            catch (Exception)
            {
                // No Cloudflare found – safe to ignore
            }
        }
    }
}