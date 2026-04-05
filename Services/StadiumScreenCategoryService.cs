using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using WeBook.Models;

namespace WeBook.Services
{
    public static class StadiumScreenCategoryService
    {
        #region STADIUM SCREEN - Categories
        public static List<Category> GetCategories(IWebDriver driver)
        {
            var categories = new List<Category>();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                var expandButton = driver.FindElements(By.CssSelector("button[data-open='false']")).FirstOrDefault();
                if (expandButton != null && expandButton.Displayed)
                {
                    expandButton.Click();
                    wait.Until(d => d.FindElements(By.CssSelector("button[data-open='true']")).Count > 0);
                    Thread.Sleep(500);
                }
            }
            catch { }

            wait.Until(d => d.FindElements(By.CssSelector("li.py-2")).Count > 0);

            wait.Until(d =>
            {
                var firstItem = d.FindElements(By.CssSelector("li.py-2")).FirstOrDefault();
                if (firstItem == null) return false;
                var nameElem = firstItem.FindElements(By.CssSelector(".grow.text-sm p")).FirstOrDefault();
                return nameElem != null && !string.IsNullOrEmpty(nameElem.Text);
            });

            var categoryItems = driver.FindElements(By.CssSelector("li.py-2"));
            Console.WriteLine($"Found {categoryItems.Count} category items.");

            foreach (var item in categoryItems)
            {
                try
                {
                    var colorDiv = item.FindElement(By.CssSelector(".flex.h-5.w-5.shrink-0.items-center.justify-center.rounded.p-1"));
                    string bgColor = colorDiv.GetCssValue("background-color");
                    var rgb = ParseRgbString(bgColor);

                    var nameElem = item.FindElement(By.CssSelector(".grow.text-sm p"));
                    string name = nameElem.Text.Trim();

                    categories.Add(new Category { Name = name, Rgb = rgb });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing category: {ex.Message}");
                    categories.Add(new Category { Name = "Error", Rgb = new int[] { 0, 0, 0 } });
                }
            }

            if (categories.Any(c => c.Name == "Unknown") && categoryItems.Count > 0)
            {
                Console.WriteLine("Debug: Could not extract names. First item HTML:");
                Console.WriteLine(categoryItems.First().GetAttribute("outerHTML"));
            }

            //CLOSE THE DROPDOWN TO PREVENT ISSUES WITH IFRAME DETECTION
            CloseCategoryDropdown(driver);
            return categories;
        }

        private static int[] ParseRgbString(string bgColor)
        {
            var match = Regex.Match(bgColor, @"rgba?\((\d+),\s*(\d+),\s*(\d+)");
            if (match.Success)
            {
                return new int[]
                {
                        int.Parse(match.Groups[1].Value),
                        int.Parse(match.Groups[2].Value),
                        int.Parse(match.Groups[3].Value)
                };
            }
            throw new Exception($"Could not parse color: {bgColor}");
        }

        public static void CloseCategoryDropdown(IWebDriver driver)
        {
            try
            {
                var toggle = driver.FindElement(By.CssSelector("button[data-open='true']"));
                if (toggle != null && toggle.Displayed && toggle.Enabled)
                {
                    toggle.Click();
                    Thread.Sleep(500);
                }
            }
            catch (NoSuchElementException) { }
        }

        #endregion
    }
}
