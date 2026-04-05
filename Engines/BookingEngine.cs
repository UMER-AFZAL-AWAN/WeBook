using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using WeBook.Utilities;

namespace WeBook.Engines
{
    public static class BookingEngine
    {
        public static void FinalizeReservation(IWebDriver driver, int quantity)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Logger.Log($"Confirming {quantity} seats...");

            try
            {
                // This JS snippet handles the + button and the Confirm button inside the popup
                string bookingScript = @"
                    let plus = document.getElementById('ga-increase-seats');
                    let confirm = document.getElementById('ga-confirm-seats');
                    if (confirm) {
                        for(let i = 1; i < arguments[0]; i++) { if(plus) plus.click(); }
                        confirm.click();
                        return true;
                    }
                    return false;";

                js.ExecuteScript(bookingScript, quantity);
                Thread.Sleep(2000);

                // Find and click the final Checkout button in the main UI
                var checkout = driver.FindElements(By.XPath("//button[contains(., 'Checkout')]")).FirstOrDefault();
                if (checkout != null)
                {
                    checkout.Click();
                    Logger.Log("🚀 Success! Seats secured. Redirecting to payment.");
                }
            }
            catch (Exception ex) { Logger.Log($"Booking Error: {ex.Message}"); }
        }



        public static void HandleCookieBanner(IWebDriver driver)
        {
            try
            {
                // Using a broad XPath to find "Reject" or "Accept Essential"
                var cookieBtn = driver.FindElements(By.XPath("//button[contains(., 'Reject') or contains(., 'Essential')]")).FirstOrDefault();
                if (cookieBtn != null && cookieBtn.Displayed)
                {
                    cookieBtn.Click();
                    Logger.Log("✅ Cookies handled. View cleared.");
                }
            }
            catch { /* Banner not present, proceed */ }
        }
    }
}