using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Core;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class BookingService
    {
        public static void HandleCookieBanner(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
            InteractionHelper.Initialize(driver);

            try
            {
                Logger.Log("Waiting for cookie banner...");
                var rejectButton = wait.Until(d => d.FindElement(By.XPath("//button[contains(., 'Reject all non-essential')]")));
                InteractionHelper.Click(rejectButton);
                Logger.Log("✅ Cookies rejected.");
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Logger.Log($"ℹ️ Cookie banner handled or not found: {ex.Message}");
            }
            CloudflareGuard.Check(driver);
        }

        public static void ConfirmTeamSelection(IWebDriver driver)
        {
            try
            {
                var localWait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
                var confirmBtn = localWait.Until(d => d.FindElement(By.CssSelector("button[class*='confirm'], .btn-primary")));
                if (confirmBtn.Displayed)
                {
                    InteractionHelper.Initialize(driver);
                    InteractionHelper.Click(confirmBtn, useJS: true);
                    Logger.Log("✅ Team selection confirmed.");
                }
            }
            catch (WebDriverTimeoutException) { }
            catch (Exception ex)
            {
                Logger.Log("ℹ️ Team selection check skipped: " + ex.Message);
            }
            CloudflareGuard.Check(driver);
        }

        public static void ClickCheckout(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            InteractionHelper.Initialize(driver);

            try
            {
                var checkoutBtn = wait.Until(d => d.FindElement(By.CssSelector("button[class*='bg-primary'], button[data-testid='checkout-button']")));
                InteractionHelper.ScrollToElement(checkoutBtn);
                InteractionHelper.Click(checkoutBtn);
                Logger.Log("✅ Checkout clicked.");
            }
            catch (Exception ex)
            {
                Logger.Log("❌ Could not find checkout button: " + ex.Message);
            }
            CloudflareGuard.Check(driver);
        }
    }
}