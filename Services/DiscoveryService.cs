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
        // This opens the sidebar so the HTML sections exist for the scraper
        public static void EnsureEnclosureTabOpen(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                // Look for the toggle button (z-50 flex is common for this UI)
                var toggleButton = driver.FindElements(By.CssSelector("button[class*='z-50']")).FirstOrDefault();

                if (toggleButton != null)
                {
                    toggleButton.Click();
                    Logger.Log("✅ Enclosure tab interaction triggered.");
                    Thread.Sleep(2000); // Wait for animation
                }
            }
            catch (Exception ex)
            {
                Logger.Log("ℹ️ Enclosure tab check skipped: " + ex.Message);
            }
        }

        public static List<SeatSelection> ScanAvailableSections(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            // Wait for the legend container to actually exist in the DOM
            wait.Until(d => d.FindElements(By.Id("legend")).Count > 0);

            var sections = new List<SeatSelection>();
            var legendItems = driver.FindElements(By.CssSelector("#legend .legend-item"));

            foreach (var item in legendItems)
            {
                string text = item.Text.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                // FIX: Use double quotes for strings and char for single characters
                // Use StringSplitOptions to handle potential empty entries
                string sectionName = text.Split(new[] { "\n" }, StringSplitOptions.None)[0];

                // FIX: 'SAR' is 3 characters, so it MUST be a string "SAR"
                string pricePart = text.Contains("SAR")
                    ? text.Split(new[] { "SAR" }, StringSplitOptions.None)[0].Trim()
                    : "Unknown";

                sections.Add(new SeatSelection
                {
                    Section = sectionName,
                    Price = pricePart
                });
            }
            return sections;
        }
    }
}