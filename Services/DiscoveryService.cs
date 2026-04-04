//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Support.UI;
//using WeBook.Models;
//using WeBook.Utilities;

//namespace WeBook.Services
//{
//    public static class DiscoveryService
//    {
//        public static void EnsureEnclosureTabOpen(IWebDriver driver)
//        {
//            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
//            try
//            {
//                var toggleButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
//                    By.CssSelector("button[class*='z-50'][class*='flex']")));

//                string isOpen = toggleButton.GetAttribute("data-open") ?? "false";
//                if (isOpen.ToLower() == "false")
//                {
//                    toggleButton.Click();
//                    Logger.Log("✅ Enclosure tab expanded.");
//                    Thread.Sleep(1000);
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("ℹ️ Enclosure tab check: " + ex.Message);
//            }
//        }

//        public static List<SeatRequest> GetAllEnclosures(IWebDriver driver)
//        {
//            var uniqueEnclosures = new Dictionary<string, SeatRequest>(StringComparer.OrdinalIgnoreCase);
//            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
//            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

//            try
//            {
//                // Target the specific scroll container found in your HTML
//                var scrollContainer = wait.Until(d => d.FindElement(By.CssSelector("div.mini-scrollbar")));

//                int previousCount = -1;
//                int stallCount = 0;

//                // Keep scrolling until no new sections are found for 5 consecutive attempts
//                while (stallCount < 5)
//                {
//                    // Select all list items (li) inside the scrollable area
//                    var listItems = scrollContainer.FindElements(By.CssSelector("li.py-2"));

//                    foreach (var li in listItems)
//                    {
//                        try
//                        {
//                            // Extract Label (e.g., S4, G1, B1)
//                            string label = li.FindElement(By.CssSelector("div.grow.text-sm p")).Text.Trim();

//                            // Extract Price (e.g., 3,753.57)
//                            string priceText = li.FindElement(By.CssSelector("span.text-body-M")).Text.Trim();

//                            if (!uniqueEnclosures.ContainsKey(label))
//                            {
//                                uniqueEnclosures.Add(label, new SeatRequest
//                                {
//                                    Section = label,
//                                    Price = priceText.Replace(",", "")
//                                });
//                            }
//                        }
//                        catch { continue; } // Item might be mid-render, skip to next
//                    }

//                    if (uniqueEnclosures.Count == previousCount)
//                    {
//                        stallCount++;
//                    }
//                    else
//                    {
//                        stallCount = 0;
//                        previousCount = uniqueEnclosures.Count;
//                    }

//                    // TRIGGER LAZY LOAD: Scroll down in 400px increments
//                    js.ExecuteScript("arguments[0].scrollBy(0, 400);", scrollContainer);
//                    Thread.Sleep(800); // Wait for the 'Cat' headers and items to load
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("⚠️ Discovery Error: " + ex.Message);
//            }

//            return uniqueEnclosures.Values.ToList();
//        }
//    }
//}