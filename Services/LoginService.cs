using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using WeBook.Helpers;

namespace WeBook.Services
{
    public class LoginService
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginService(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        public bool Login(string email, string password)
        {
            try
            {
                Logger.Step("Attempting login...");

                var emailInput = _wait.Until(d => d.FindElement(By.CssSelector("[data-testid='auth_login_email_input']")));
                emailInput.SendKeys(email);
                Logger.Debug("Email entered");

                var passwordInput = _driver.FindElement(By.CssSelector("[data-testid='auth_login_password_input']"));
                passwordInput.SendKeys(password);
                Logger.Debug("Password entered");

                var submitButton = _driver.FindElement(By.CssSelector("[data-testid='auth_login_submit_button']"));
                submitButton.Click();

                Logger.Success("Login submitted (handle CAPTCHA manually)");
                System.Threading.Thread.Sleep(5000);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Login failed: {ex.Message}");
                return false;
            }
        }
    }
}