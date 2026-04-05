using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Utilities;
using WeBook.Core;
using WeBook.Models;

namespace WeBook.Services
{
    public class AuthenticationService : BaseService
    {
        public void LoginAndSolve(IWebDriver driver, UserCredentials creds)
        {
            var js = GetJs(driver);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            try
            {
                // Wait for fields to be interactable
                var emailField = wait.Until(d => d.FindElement(By.CssSelector(Selectors.LoginEmailInput)));
                var passwordField = driver.FindElement(By.CssSelector("input[type='password']"));
                var loginBtn = driver.FindElement(By.CssSelector("button[type='submit']"));

                Logger.Log("?? Step 1: Form detected. Simulating keystrokes...");

                // Clear and Type Email like a human
                emailField.Click();
                emailField.Clear();
                emailField.SendKeys(creds.Email);

                // Clear and Type Password
                passwordField.Click();
                passwordField.Clear();
                passwordField.SendKeys(creds.Password);

                Thread.Sleep(1000); // Give the site's JS a second to validate the input

                // Final check: Use JS to click the button if a normal click is blocked
                js.ExecuteScript("arguments[0].click();", loginBtn);

                Logger.Log("?? Step 2: Login button clicked. MONITORING REDIRECT...");

                // --- THE RELIABLE VERIFIER ---
                // We wait for the login container to DISAPPEAR or the MAP to appear.
                wait.Until(d => {
                    bool mapExists = d.FindElements(By.CssSelector("div[data-testid='seat-chart'], canvas, .stadium-map")).Count > 0;
                    bool loginBoxGone = d.FindElements(By.CssSelector(Selectors.LoginEmailInput)).Count == 0;
                    return mapExists || loginBoxGone;
                });

                Thread.Sleep(3000);
                Logger.Log("?? Step 3: Login officially CLEARED.");
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ LOGIN BLOCKER: {ex.Message}");
                throw;
            }

            SolveCloudflare(driver);
        }

        public void SolveCloudflare(IWebDriver driver)
        {
            var js = GetJs(driver);
            var cfFrames = driver.FindElements(By.CssSelector("iframe[title*='Cloudflare']"));

            if (cfFrames.Count > 0)
            {
                Logger.Log("⚠️ Security check detected...");
                try
                {
                    driver.SwitchTo().Frame(cfFrames[0]);
                    var checkbox = driver.FindElements(By.CssSelector("#challenge-stage")).FirstOrDefault();
                    if (checkbox != null) js.ExecuteScript("arguments[0].click();", checkbox);
                }
                finally { driver.SwitchTo().DefaultContent(); }
                Thread.Sleep(5000);
            }
        }
    }
}