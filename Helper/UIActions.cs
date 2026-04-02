using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace WeBook.Helper
{
    public static class UIActions
    {
        public static void Login(IWebDriver driver, WebDriverWait wait, string email, string password)
        {
            try
            {
                var reject = wait.Until(d => d.FindElements(By.XPath("//button[contains(., 'Reject')]")).FirstOrDefault());
                reject?.Click();
                Thread.Sleep(1000);

                wait.Until(d => d.FindElement(By.CssSelector("input[data-testid='auth_login_email_input']"))).SendKeys(email);
                driver.FindElement(By.CssSelector("input[data-testid='auth_login_password_input']")).SendKeys(password);
                driver.FindElement(By.CssSelector("button[data-testid='auth_login_submit_button']")).Click();
                Thread.Sleep(3000);
            }
            catch
            {
                Console.WriteLine("ℹ️ Login elements not found (perhaps already logged in).");
            }
        }

        public static void WaitForPageLoad(IWebDriver driver, int timeoutSeconds = 10)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }

        // --- Team Selection ---
        public static bool IsTeamSelectionScreen(IWebDriver driver)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var buttons = wait.Until(d => d.FindElements(By.CssSelector("button[data-testid^='ui_toggle_favorite_team_']")));
                return buttons.Count > 0;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public static void HandleTeamSelection(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            var teamButtons = driver.FindElements(By.CssSelector("button[data-testid^='ui_toggle_favorite_team_']"));
            if (teamButtons.Count == 0)
            {
                Console.WriteLine("No team selection buttons found. Skipping team selection.");
                return;
            }

            var teams = new List<string>();
            foreach (var btn in teamButtons)
            {
                var img = btn.FindElement(By.TagName("img"));
                string teamName = img.GetAttribute("alt");
                teams.Add(teamName);
            }

            Console.WriteLine("\nChoose your favorite team:");
            for (int i = 0; i < teams.Count; i++)
                Console.WriteLine($"{i + 1}. {teams[i]}");
            Console.Write("Enter team number: ");
            int choice = int.Parse(Console.ReadLine()) - 1;
            if (choice < 0 || choice >= teams.Count)
            {
                Console.WriteLine("Invalid choice. Exiting.");
                return;
            }

            teamButtons[choice].Click();
            Thread.Sleep(500);

            var checkbox = driver.FindElement(By.CssSelector("button[data-testid='ticketing_teams_terms_checkbox']"));
            if (checkbox.GetAttribute("data-state") != "checked")
            {
                checkbox.Click();
                Thread.Sleep(500);
            }

            var nextButton = wait.Until(drv => drv.FindElement(By.XPath("//button[contains(., 'Next: Select Tickets')]")));
            wait.Until(ExpectedConditions.ElementToBeClickable(nextButton));
            nextButton.Click();

            wait.Until(drv => drv.FindElements(By.CssSelector("button[data-open]")).Count > 0 ||
                             drv.FindElements(By.Id("seats-cloud-chart")).Count > 0);
            Thread.Sleep(2000);
        }

        // --- Categories ---
        //public static List<Category> GetCategories(IWebDriver driver)
        //{
        //    var categories = new List<Category>();

        //    // Expand the category dropdown if collapsed
        //    var expandButton = driver.FindElements(By.CssSelector("button[data-open='false']")).FirstOrDefault();
        //    if (expandButton != null && expandButton.Displayed)
        //    {
        //        expandButton.Click();
        //        Thread.Sleep(500);
        //    }

        //    // Wait for the category list to be visible
        //    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        //    wait.Until(d => d.FindElements(By.CssSelector("li.py-2")).Count > 0);

        //    var categoryItems = driver.FindElements(By.CssSelector("li.py-2"));
        //    foreach (var item in categoryItems)
        //    {
        //        var colorDiv = item.FindElement(By.CssSelector(".flex.h-5.w-5.shrink-0.items-center.justify-center.rounded.p-1"));
        //        string bgColor = colorDiv.GetCssValue("background-color");
        //        var rgb = ParseRgbString(bgColor);

        //        var nameDiv = item.FindElement(By.CssSelector(".grow.text-sm p"));
        //        string name = nameDiv.Text.Trim();

        //        categories.Add(new Category { Name = name, Rgb = rgb });
        //    }
        //    return categories;
        //}
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

            return categories;
        }
        public static void ClickCanvasAtColor(IWebDriver driver, int[] targetRgb, int tolerance = 15)
        {
            var js = (IJavaScriptExecutor)driver;
            string script = @"
        var targetR = arguments[0];
        var targetG = arguments[1];
        var targetB = arguments[2];
        var tol = arguments[3];
        var canvas = document.querySelector('canvas');
        if (!canvas) return false;
        var rect = canvas.getBoundingClientRect();
        var ctx = canvas.getContext('2d', {willReadFrequently: true});
        var width = canvas.width;
        var height = canvas.height;
        var data = ctx.getImageData(0, 0, width, height).data;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var idx = (y * width + x) * 4;
                if (Math.abs(data[idx] - targetR) <= tol &&
                    Math.abs(data[idx+1] - targetG) <= tol &&
                    Math.abs(data[idx+2] - targetB) <= tol) {
                    var cX = rect.left + x * (rect.width / width);
                    var cY = rect.top + y * (rect.height / height);
                    var ev = new MouseEvent('click', { view: window, bubbles: true, clientX: cX, clientY: cY });
                    canvas.dispatchEvent(ev);
                    return true;
                }
            }
        }
        return false;
    ";
            js.ExecuteScript(script, targetRgb[0], targetRgb[1], targetRgb[2], tolerance);
        }
        public static void ClickCategory(IWebDriver driver, Category category)
        {
            // Re-find the category element by its name
            var categoryItem = driver.FindElements(By.CssSelector("li.py-2"))
                                     .FirstOrDefault(li => li.FindElement(By.CssSelector(".grow.text-sm p")).Text == category.Name);
            if (categoryItem != null)
            {
                categoryItem.Click();
            }
            else
            {
                throw new Exception($"Category '{category.Name}' not found.");
            }
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

        // --- Canvas helpers ---
        public static int[] GetCanvasCenterColor(IWebDriver driver)
        {
            var js = (IJavaScriptExecutor)driver;
            string script = @"
                var canvas = document.querySelector('canvas');
                if (!canvas) return null;
                var ctx = canvas.getContext('2d', {willReadFrequently: true});
                var midX = Math.floor(canvas.width/2);
                var midY = Math.floor(canvas.height/2);
                var data = ctx.getImageData(0, 0, canvas.width, canvas.height).data;
                var idx = (midY * canvas.width + midX) * 4;
                return [data[idx], data[idx+1], data[idx+2]];
            ";
            var result = js.ExecuteScript(script) as object[];
            if (result != null && result.Length == 3)
                return new int[] { Convert.ToInt32(result[0]), Convert.ToInt32(result[1]), Convert.ToInt32(result[2]) };
            return null;
        }

        public static void WaitForCanvasUpdate(IWebDriver driver, int[] previousSample)
        {
            if (previousSample == null) return;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d =>
            {
                var current = GetCanvasCenterColor(d);
                if (current == null) return false;
                return !(current[0] == previousSample[0] &&
                         current[1] == previousSample[1] &&
                         current[2] == previousSample[2]);
            });
        }

        public static int[] GetActualCategoryColor(IWebDriver driver)
        {
            var js = (IJavaScriptExecutor)driver;
            string script = @"
                var canvas = document.querySelector('canvas');
                if (!canvas) return null;
                var ctx = canvas.getContext('2d', {willReadFrequently: true});
                var width = canvas.width;
                var height = canvas.height;
                var data = ctx.getImageData(0, 0, width, height).data;
                for (var i = 0; i < data.length; i += 4) {
                    var r = data[i];
                    var g = data[i+1];
                    var b = data[i+2];
                    if (r > 50 || g > 50 || b > 50) {
                        return [r, g, b];
                    }
                }
                return null;
            ";
            var result = js.ExecuteScript(script) as object[];
            if (result != null && result.Length == 3)
                return new int[] { Convert.ToInt32(result[0]), Convert.ToInt32(result[1]), Convert.ToInt32(result[2]) };
            return null;
        }

        // --- GA Popup ---
        public static bool IsGAPopupPresent(IWebDriver driver)
        {
            try
            {
                return driver.FindElements(By.CssSelector(".ga-popup-overlay, #ga-popup:not([hidden])")).Count > 0;
            }
            catch { return false; }
        }

        public static void HandleGAPopup(IWebDriver driver, int seatCount)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            wait.Until(d => d.FindElement(By.CssSelector(".ga-popup-overlay, #ga-popup")).Displayed);

            // Use the correct input selector – try common ones
            var quantityInput = driver.FindElements(By.CssSelector(".ga-popup-content input[type='number'], #ga-seat-count")).FirstOrDefault();
            if (quantityInput == null) throw new Exception("Quantity input not found in GA popup");

            quantityInput.Clear();
            quantityInput.SendKeys(seatCount.ToString());

            var confirmBtn = driver.FindElements(By.CssSelector(".ga-popup-content button:contains('Confirm'), #ga-confirm-seats")).FirstOrDefault();
            if (confirmBtn == null) throw new Exception("Confirm button not found in GA popup");
            confirmBtn.Click();

            wait.Until(d => d.FindElements(By.CssSelector(".ga-popup-overlay, #ga-popup:not([hidden])")).Count == 0);
        }

        // --- Seat selection ---
        public static void ClickSeat(IWebDriver driver, Seat seat)
        {
            var js = (IJavaScriptExecutor)driver;
            string script = @"
                var canvas = document.querySelector('canvas');
                var rect = canvas.getBoundingClientRect();
                var sX = rect.width / canvas.width;
                var sY = rect.height / canvas.height;
                var bX = arguments[0];
                var bY = arguments[1];
                var pts = [[0,0], [4,0], [0,4], [-4,0], [0,-4], [4,4], [-4,-4], [4,-4], [-4,4]];
                pts.forEach(function(p) {
                    var cX = rect.left + (bX + p[0]) * sX;
                    var cY = rect.top + (bY + p[1]) * sY;
                    ['pointerdown', 'mousedown', 'pointerup', 'mouseup', 'click'].forEach(t => {
                        var cls = t.includes('pointer') ? PointerEvent : MouseEvent;
                        canvas.dispatchEvent(new cls(t, { view: window, bubbles: true, clientX: cX, clientY: cY, buttons: 1 }));
                    });
                });
            ";
            js.ExecuteScript(script, seat.CentreX, seat.CentreY);
            Thread.Sleep(200);
        }

        // --- Checkout ---
        public static void ClickCheckout(IWebDriver driver)
        {
            driver.SwitchTo().DefaultContent();
            var checkoutBtn = driver.FindElements(By.XPath("//button[contains(., 'Next') or contains(., 'Checkout')]"))
                                    .FirstOrDefault(b => b.Enabled && b.Displayed);
            if (checkoutBtn != null)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", checkoutBtn);
            }
            else
            {
                throw new Exception("Checkout button not found.");
            }
        }

        // --- Iframe handling ---
        public static IWebElement FindMapIframe(IWebDriver driver)
        {
            driver.SwitchTo().DefaultContent();
            CloseCategoryDropdown(driver);
            Thread.Sleep(500);

            var frames = driver.FindElements(By.TagName("iframe"));
            foreach (var f in frames)
            {
                try
                {
                    driver.SwitchTo().Frame(f);
                    if (driver.FindElements(By.TagName("canvas")).Count > 0)
                    {
                        driver.SwitchTo().DefaultContent();
                        return f;
                    }
                    driver.SwitchTo().DefaultContent();
                }
                catch
                {
                    driver.SwitchTo().DefaultContent();9
                }
            }
            return null;
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
    }
}