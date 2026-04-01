using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using WeBook.Models;
using System.Threading;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class AuthService
    {
        public static void Login(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                // 1. Wait for the email field to be 'Visible' and 'Enabled'
                // This bypasses issues where the element exists in HTML but isn't ready for input
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

                // 2. Professional Wait: Wait for the login overlay to actually DISAPPEAR
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
                // 1. Give the page a generous 5 seconds to load the security widget
                Logger.Log("Security page detected. Polling for verification widget...");
                Thread.Sleep(20000);

                // 2. Look for ANY iframe that might be Cloudflare without crashing if missing
                var iframes = driver.FindElements(By.XPath("//iframe[contains(@src, 'cloudflare') or contains(@title, 'Cloudflare')]"));

                if (iframes.Count > 0)
                {
                    driver.SwitchTo().Frame(iframes[0]);
                    Logger.Log("Switched to Security Frame.");

                    // Try to find the checkbox, but only wait 5 seconds
                    var shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                    var checkbox = shortWait.Until(d => d.FindElement(By.CssSelector("input[type='checkbox'], .cb-i, #challenge-stage")));

                    checkbox.Click();
                    Logger.Log("✅ Verification clicked.");

                    // Return to the main page
                    driver.SwitchTo().DefaultContent();
                }
                else
                {
                    Logger.Log("ℹ️ No Cloudflare iframe found. It may have cleared automatically or requires manual click.");
                }
            }
            catch (Exception ex)
            {
                // If anything goes wrong, we ALWAYS go back to default content and let the script continue
                driver.SwitchTo().DefaultContent();
                Logger.Log("ℹ️ Security step bypassed (Manual intervention might be needed): " + ex.Message);
            }
        }
    }
}
