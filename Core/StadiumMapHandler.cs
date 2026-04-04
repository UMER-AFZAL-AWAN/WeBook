using System;
using System.Threading;
using OpenQA.Selenium;
using WeBook.Core;
using WeBook.Utilities;

namespace WeBook.Engines
{
    public static class StadiumMapHandler
    {
        public static void ReserveWithGaPopup(IWebDriver driver, string sectionLabel, int quantity)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            InteractionHelper.Initialize(driver);

            // Step 1: Close any section overlay
            InteractionHelper.HideOverlay("div[data-open='true']");
            Thread.Sleep(1000);

            // Step 2: Find canvas
            IWebElement canvas = FindCanvas(driver);
            if (canvas == null) return;

            // Step 3: Click canvas center
            InteractionHelper.ScrollToElement(canvas);
            InteractionHelper.ClickCenter(canvas);
            Logger.Log($"✅ Canvas clicked for {sectionLabel}. Waiting for GA popup...");
            Thread.Sleep(2000);

            // Step 4: Use GA popup logic (from PopupEngine + SeatEngine script)
            try
            {
                string script = @"
                    let confirmBtn = document.getElementById('ga-confirm-seats');
                    let plusBtn = document.getElementById('ga-increase-seats');
                    
                    if (confirmBtn) {
                        let targetQty = arguments[0];
                        for(let i = 1; i < targetQty; i++) {
                            if(plusBtn) plusBtn.click();
                        }
                        confirmBtn.click();
                        return true;
                    }
                    return false;";

                bool success = (bool)js.ExecuteScript(script, quantity);
                if (success)
                    Logger.Log($"✅ {quantity} seats added via GA popup for {sectionLabel}.");
                else
                    Logger.Log("⚠️ GA popup buttons not found.");
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ GA popup script failed: {ex.Message}");
            }
        }

        public static void ReserveWithGenericPopup(IWebDriver driver, int quantity)
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));
            InteractionHelper.Initialize(driver);

            try
            {
                var popup = wait.Until(d => d.FindElement(By.Id("ga-popup")));
                var quantityInput = driver.FindElement(By.Id("ga-seat-count"));
                InteractionHelper.ExecuteScript("arguments[0].value = arguments[1];", quantityInput, quantity);
                InteractionHelper.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", quantityInput);
                Logger.Log($"Quantity set to {quantity}.");

                var confirmBtn = driver.FindElement(By.Id("ga-confirm-seats"));
                InteractionHelper.Click(confirmBtn);
            }
            catch (Exception ex)
            {
                Logger.Log($"Generic popup error: {ex.Message}");
            }
        }

        private static IWebElement FindCanvas(IWebDriver driver)
        {
            for (int i = 0; i < 3; i++)
            {
                var canvases = driver.FindElements(By.TagName("canvas"));
                if (canvases.Count > 0)
                    return canvases[0];

                // Search inside iframes
                var frames = driver.FindElements(By.TagName("iframe"));
                foreach (var frame in frames)
                {
                    try
                    {
                        driver.SwitchTo().Frame(frame);
                        var frameCanvases = driver.FindElements(By.TagName("canvas"));
                        if (frameCanvases.Count > 0)
                        {
                            driver.SwitchTo().DefaultContent();
                            return frameCanvases[0];
                        }
                        driver.SwitchTo().DefaultContent();
                    }
                    catch { driver.SwitchTo().DefaultContent(); }
                }
                Logger.Log($"Retry {i + 1}: Canvas not found.");
                Thread.Sleep(3000);
                CloudflareGuard.Check(driver);
            }
            Logger.Log("❌ Canvas not found after retries.");
            return null;
        }
    }
}