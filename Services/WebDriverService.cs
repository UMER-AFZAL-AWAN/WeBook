using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Net;
using WeBook.Helpers;

namespace WeBook.Services
{
    public class WebDriverService : IDisposable
    {
        public IWebDriver Driver { get; private set; }

        public WebDriverService()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            // Add timeout to avoid hanging
            options.AddArgument("--page-load-strategy=normal");

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            Logger.Info("WebDriver initialized");
        }

        public bool NavigateToUrl(string url)
        {
            try
            {
                // Validate URL format
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    Logger.Error($"Invalid URL format: {url}");
                    return false;
                }

                Logger.Info($"Navigating to: {url}");
                Driver.Navigate().GoToUrl(url);

                // Wait a bit for page to start loading
                System.Threading.Thread.Sleep(2000);
                return true;
            }
            catch (WebDriverException ex)
            {
                if (ex.Message.Contains("ERR_NAME_NOT_RESOLVED"))
                {
                    Logger.Error($"Cannot resolve hostname. Please check:");
                    Logger.Error($"  1. Your internet connection");
                    Logger.Error($"  2. The URL is correct: {url}");
                    Logger.Error($"  3. The website is accessible");
                    Logger.Error($"  4. VPN/Proxy settings if any");
                }
                else
                {
                    Logger.Error($"Navigation error: {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected navigation error: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                Driver?.Quit();
                Driver?.Dispose();
                Logger.Info("WebDriver disposed");
            }
            catch { }
        }
    }
}