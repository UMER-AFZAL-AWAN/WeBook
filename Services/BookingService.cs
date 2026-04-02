using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class BookingService
    {
        public static void HandleCookieBanner(IWebDriver driver)
        {
                // Use a dedicated long wait for slow connections
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
            
            try
            {
                Logger.Log("Waiting for cookie banner to load");

                // Matches the exact text from your uploaded HTML
                var rejectButton = wait.Until(d => d.FindElement(By.XPath("//button[contains(., 'Reject all non-essential')]")));

                rejectButton.Click();
                Console.WriteLine("✅ Cookies rejected. View is now clear.");

                //Logger.Log("✅ Cookie banner rejectred.");

                // Give the UI a moment to remove the dark overlay
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ℹ️ Cookie banner handled or not found: {ex.Message}");
            }
        }

        // Ensure you have this method or a placeholder to avoid Program.cs errors
        public static void ConfirmTeamSelection(IWebDriver driver)
        {
            try
            {
                // Short wait so we don't hang if there is no popup
                var localWait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));

                var confirmBtn = localWait.Until(d => d.FindElement(By.CssSelector("button[class*='confirm'], .btn-primary")));

                if (confirmBtn.Displayed)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("arguments[0].click();", confirmBtn);
                    Logger.Log("✅ Team selection confirmed.");
                }
            }
            catch (WebDriverTimeoutException)
            {
                // Normal behavior: most events don't have this popup
            }
            catch (Exception ex)
            {
                Logger.Log("ℹ️ Note: Team selection check skipped: " + ex.Message);
            }
        }

        public static void ClickCheckout(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                // WeBook checkout buttons usually use a 'primary' class or a specific data-testid
                var checkoutBtn = wait.Until(d => d.FindElement(By.CssSelector("button[class*='bg-primary'], button[data-testid='checkout-button']")));

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", checkoutBtn);
                checkoutBtn.Click();
                Logger.Log("✅ Checkout clicked. Moving to payment/summary...");
            }
            catch (Exception ex) { Logger.Log("❌ Could not find checkout button: " + ex.Message); }
        }
    }
}