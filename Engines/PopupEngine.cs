using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Utilities; // Import Logger

namespace WeBook.Engines
{
    public static class PopupEngine
    {
        public static void HandleQuantityPopup(IWebDriver driver, int requestedQuantity)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            try
            {
                // Wait until the popup from your stadium HTML is actually visible
                var popup = wait.Until(d => d.FindElement(By.Id("ga-popup")));

                // Set the value directly in the <input id="ga-seat-count">
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                var quantityInput = driver.FindElement(By.Id("ga-seat-count"));

                js.ExecuteScript("arguments[0].value = arguments[1];", quantityInput, requestedQuantity);
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", quantityInput);

                Logger.Log($"Quantity set to {requestedQuantity} for stadium map.");

                // Click 'Confirm' button from your provided HTML
                var confirmBtn = driver.FindElement(By.Id("ga-confirm-seats"));
                confirmBtn.Click();
            }
            catch (Exception ex)
            {
                Logger.Log($"Popup Error: {ex.Message}");
            }
        }
    }
}