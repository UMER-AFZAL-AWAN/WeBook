using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using WeBook.Core;
using WeBook.Models;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class AuthService
    {
        public static void Login(IWebDriver driver, WebDriverWait wait)
        {
            InteractionHelper.Initialize(driver);

            try
            {
                var emailField = wait.Until(d => {
                    var el = d.FindElement(By.CssSelector("input[data-testid='auth_login_email_input']"));
                    return (el.Displayed && el.Enabled) ? el : null;
                });

                Logger.Log("Login fields detected. Proceeding with credentials...");
                emailField.Clear();
                emailField.SendKeys(Config.EMAIL);
                CloudflareGuard.Check(driver);

                var passField = driver.FindElement(By.CssSelector("input[data-testid='auth_login_password_input']"));
                passField.Clear();
                passField.SendKeys(Config.PASSWORD);
                CloudflareGuard.Check(driver);

                var submitBtn = driver.FindElement(By.CssSelector("button[data-testid='auth_login_submit_button']"));
                InteractionHelper.Click(submitBtn);

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(
                    By.CssSelector("button[data-testid='auth_login_submit_button']")));

                Logger.Log("✅ Login sequence completed.");
            }
            catch (WebDriverTimeoutException)
            {
                Logger.Log("ℹ️ Login fields not found. Assuming session is already active.");
            }
            CloudflareGuard.Check(driver);
        }
    }
}