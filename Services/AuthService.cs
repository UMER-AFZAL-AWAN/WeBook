using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;
using WeBook.Models;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class AuthService
    {
        public static void Login(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                var emailField = wait.Until(d => {
                    var el = d.FindElement(By.CssSelector("input[data-testid='auth_login_email_input']"));
                    return (el.Displayed && el.Enabled) ? el : null;
                });

                Logger.Log("Login fields detected. Proceeding with credentials...");

                emailField.Clear();
                emailField.SendKeys(Config.EMAIL);

                var passField = driver.FindElement(By.CssSelector("input[data-testid='auth_login_password_input']"));
                passField.Clear();
                passField.SendKeys(Config.PASSWORD);

                var submitBtn = driver.FindElement(By.CssSelector("button[data-testid='auth_login_submit_button']"));
                submitBtn.Click();

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(
                    By.CssSelector("button[data-testid='auth_login_submit_button']")));

                Logger.Log("✅ Login sequence completed.");
            }
            catch (WebDriverTimeoutException)
            {
                Logger.Log("ℹ️ Login fields not found. Assuming session is already active.");
            }
        }

        public static void SolveCloudflare(IWebDriver driver)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                // Flexible search for the Cloudflare iframe
                var cfFrame = driver.FindElements(By.XPath("//iframe[contains(@src, 'cloudflare')]")).FirstOrDefault();

                if (cfFrame != null)
                {
                    Logger.Log("Cloudflare iframe found. Attempting switch...");
                    driver.SwitchTo().Frame(cfFrame);

                    // Attempt to click the checkbox if visible
                    try
                    {
                        var checkbox = driver.FindElements(By.CssSelector("input[type='checkbox'], #challenge-stage")).FirstOrDefault();
                        checkbox?.Click();
                        Logger.Log("✅ Verification clicked.");
                    }
                    catch { /* Fail silently inside frame */ }

                    driver.SwitchTo().DefaultContent();
                }
                else
                {
                    Logger.Log("ℹ️ No Cloudflare iframe active. Checking for Turnstile text...");
                    if (driver.PageSource.Contains("Quick security check") || driver.PageSource.Contains("Verify you are human"))
                    {
                        Logger.Log("⚠️ Security check detected. Waiting 5s for auto-resolve...");
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (Exception)
            {
                driver.SwitchTo().DefaultContent();
                Logger.Log("ℹ️ Cloudflare check bypassed.");
            }
        }
    }
}