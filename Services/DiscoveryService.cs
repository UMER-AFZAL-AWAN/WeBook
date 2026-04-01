using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Models;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class DiscoveryService
    {
        public static void EnsureEnclosureTabOpen(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                var toggleButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("button[class*='z-50'][class*='flex']")));

                string isOpen = toggleButton.GetAttribute("data-open") ?? "false";
                if (isOpen.ToLower() == "false")
                {
                    toggleButton.Click();
                    Logger.Log("✅ Enclosure tab expanded.");
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("ℹ️ Enclosure tab check: " + ex.Message);
            }
        }

        public static List<SeatRequest> GetAllEnclosures(IWebDriver driver)
        {
            var uniqueEnclosures = new Dictionary<string, SeatRequest>();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                var scrollContainer = wait.Until(d => d.FindElement(By.CssSelector("div[class*='overflow-y-auto'], .custom-scrollbar, [data-testid='enclosure-list']")));

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                Logger.Log("Starting professional deep-scan of 200+ enclosures...");

                int scrollAttempts = 0;
                int maxScrolls = 100; // Increased for 200+ items

                for (int i = 0; i < maxScrolls; i++)
                {
                    var visibleRows = driver.FindElements(By.CssSelector("div.flex.items-center.gap-1\\.5, button[class*='cursor-pointer']"));
                    int countBefore = uniqueEnclosures.Count;

                    foreach (var row in visibleRows)
                    {
                        try
                        {
                            string rawText = row.Text;
                            if (string.IsNullOrWhiteSpace(rawText)) continue;

                            string[] parts = rawText.Split('\n');
                            string label = parts[0].Trim();
                            string price = parts.Length > 1 ? parts.Last().Trim() : "";

                            if (!string.IsNullOrEmpty(label) && !uniqueEnclosures.ContainsKey(label))
                            {
                                uniqueEnclosures.Add(label, new SeatRequest { Section = label, Price = price });
                            }
                        }
                        catch (StaleElementReferenceException) { continue; }
                    }

                    // Scroll down by the container height
                    js.ExecuteScript("arguments[0].scrollTop += 800;", scrollContainer);
                    Thread.Sleep(1200); // Give the slow connection time to fetch the next 20 sections

                    // Check if we hit the bottom
                    // The (bool?) and ?? false handles the "Unboxing" warning professionally
                    object? scrollResult = js.ExecuteScript(
                        "return (arguments[0].scrollTop + arguments[0].offsetHeight) >= (arguments[0].scrollHeight - 20);",
                        scrollContainer);

                    bool isAtBottom = (scrollResult as bool?) ?? false;

                    if (isAtBottom && uniqueEnclosures.Count == countBefore)
                    {
                        scrollAttempts++;
                        if (scrollAttempts > 2) break;
                    }
                }

                Logger.Log($"✅ Total Enclosures Captured: {uniqueEnclosures.Count}");
            }
            catch (Exception ex)
            {
                Logger.Log("⚠️ Error during deep scan: " + ex.Message);
            }

            return uniqueEnclosures.Values.ToList();
        }
    }
}