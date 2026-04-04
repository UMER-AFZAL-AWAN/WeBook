using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Core;
using WeBook.Models;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class CategorySelector
    {
        public static void OpenEnclosureTab(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            InteractionHelper.Initialize(driver);

            try
            {
                var toggleButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("button[class*='z-50'][class*='flex']")));

                string isOpen = toggleButton.GetAttribute("data-open") ?? "false";
                if (isOpen.ToLower() == "false")
                {
                    InteractionHelper.Click(toggleButton);
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
            var uniqueEnclosures = new Dictionary<string, SeatRequest>(StringComparer.OrdinalIgnoreCase);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            InteractionHelper.Initialize(driver);

            try
            {
                var scrollContainer = wait.Until(d => d.FindElement(By.CssSelector("div.mini-scrollbar")));
                int previousCount = -1;
                int stallCount = 0;

                while (stallCount < 5)
                {
                    var listItems = scrollContainer.FindElements(By.CssSelector("li.py-2"));
                    foreach (var li in listItems)
                    {
                        try
                        {
                            string label = li.FindElement(By.CssSelector("div.grow.text-sm p")).Text.Trim();
                            string priceText = li.FindElement(By.CssSelector("span.text-body-M")).Text.Trim();
                            if (!uniqueEnclosures.ContainsKey(label))
                            {
                                uniqueEnclosures.Add(label, new SeatRequest
                                {
                                    Section = label,
                                    Price = priceText.Replace(",", "")
                                });
                            }
                        }
                        catch { }
                    }

                    if (uniqueEnclosures.Count == previousCount)
                        stallCount++;
                    else
                    {
                        stallCount = 0;
                        previousCount = uniqueEnclosures.Count;
                    }

                    InteractionHelper.ScrollContainerBy(scrollContainer, 400);
                    Thread.Sleep(800);
                    CloudflareGuard.Check(driver);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("⚠️ Discovery Error: " + ex.Message);
            }

            return uniqueEnclosures.Values.ToList();
        }
    }
}