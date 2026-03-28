//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.Linq;
//using System.Threading;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Interactions;
//using OpenQA.Selenium.Support.UI;

//namespace WeBookAutomation
//{
//    class Program
//    {
//        // --- CONFIGURATION ---
//        const string EMAIL = "cabnipcar@bangban.uk";
//        const string PASSWORD = "Aa@123456789";
//        const string URL = "https://webook.com/en/events/rsl-al-khaleej-vs-al-hilal-387468/book";
//        static readonly int[] TargetRGB = { 139, 195, 74 };

//        static void Main(string[] args)
//        {
//            // Clean up existing processes to prevent 'Resource Busy' errors
//            foreach (var p in Process.GetProcessesByName("chromedriver")) { try { p.Kill(); } catch { } }

//            var options = new ChromeOptions();
//            options.AddArgument("--start-maximized");
//            options.AddArgument("--force-device-scale-factor=1");
//            options.AddArgument("--disable-blink-features=AutomationControlled");

//            // REMOVED 'using' block: This prevents the driver from auto-closing when Main finishes
//            IWebDriver driver = new ChromeDriver(options);
//            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

//            try
//            {
//                Console.WriteLine("🚀 Workflow Started...");
//                driver.Navigate().GoToUrl(URL);

//                // --- 1. LOGIN ---
//                HandleLogin(driver, wait);

//                // --- 2. IFRAME DETECTION ---
//                IWebElement mapIframe = FindMapIframe(driver);
//                if (mapIframe == null) { Console.WriteLine("❌ Map Not Found."); return; }

//                // --- 3. THE TWO-STEP SELECTION (ZOOM then SPIRAL) ---

//                // STEP A: Initial Section Zoom
//                Console.WriteLine("🎯 Zooming to section...");
//                var zoomCoords = GetSeatCoords(driver, 20);
//                if (zoomCoords != null)
//                {
//                    TriggerPreciseClick(driver, zoomCoords);
//                    // Crucial: Wait for the zoom animation to finish so the canvas stabilizes
//                    Thread.Sleep(5000);
//                }

//                // STEP B: Precise Seat Selection
//                Console.WriteLine("💺 Selecting seat with Spiral Precision...");
//                for (int attempt = 0; attempt < 40; attempt++)
//                {
//                    var seatCoords = GetSeatCoords(driver, 35);
//                    if (seatCoords != null)
//                    {
//                        TriggerPreciseClick(driver, seatCoords);

//                        // Check main page for the 'Checkout' button
//                        driver.SwitchTo().DefaultContent();
//                        var checkoutBtn = driver.FindElements(By.XPath("//button[contains(., 'Next') or contains(., 'Checkout')]"))
//                                                .FirstOrDefault(b => b.Enabled && b.Displayed);

//                        if (checkoutBtn != null)
//                        {
//                            Console.WriteLine("🎉 SUCCESS: Seat Captured!");
//                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", checkoutBtn);

//                            // Handle final checkbox
//                            try
//                            {
//                                var cb = wait.Until(d => d.FindElement(By.CssSelector("input[type='checkbox']")));
//                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cb);
//                            }
//                            catch { }
//                            break;
//                        }
//                        driver.SwitchTo().Frame(mapIframe);
//                    }
//                    Thread.Sleep(500);
//                }
//            }
//            catch (Exception e) { Console.WriteLine($"❌ Error: {e.Message}"); }

//            // --- THE 'STAY OPEN' MECHANISM ---
//            Console.WriteLine("\n✅ Execution Finished. Window will remain open.");
//            Console.WriteLine("Press Ctrl+C in this console to kill the browser.");

//            // This infinite loop keeps the C# process alive, 
//            // and because we didn't 'Dispose' the driver, the Chrome window stays.
//            while (true)
//            {
//                Thread.Sleep(10000);
//            }
//        }

//        // --- HELPER METHODS (Same logic as previous, professionally commented) ---

//        static void HandleLogin(IWebDriver driver, WebDriverWait wait)
//        {
//            try
//            {
//                var reject = wait.Until(d => d.FindElements(By.XPath("//button[contains(., 'Reject')]")).FirstOrDefault());
//                reject?.Click();

//                wait.Until(d => d.FindElement(By.CssSelector("input[data-testid='auth_login_email_input']"))).SendKeys(EMAIL);
//                driver.FindElement(By.CssSelector("input[data-testid='auth_login_password_input']")).SendKeys(PASSWORD);
//                driver.FindElement(By.CssSelector("button[data-testid='auth_login_submit_button']")).Click();
//                Thread.Sleep(3000);
//            }
//            catch { }
//        }

//        static Dictionary<string, object> GetSeatCoords(IWebDriver driver, int tolerance)
//        {
//            var js = (IJavaScriptExecutor)driver;
//            string script = @"
//                var canvas = document.querySelector('canvas');
//                if (!canvas) return null;
//                var ctx = canvas.getContext('2d', {willReadFrequently: true});
//                var data = ctx.getImageData(0, 0, canvas.width, canvas.height).data;
//                var tr = arguments[0], tg = arguments[1], tb = arguments[2], tol = arguments[3];
//                for (var i = 0; i < data.length; i += 16) {
//                    if (Math.abs(data[i]-tr)<tol && Math.abs(data[i+1]-tg)<tol && Math.abs(data[i+2]-tb)<tol) {
//                        return { 'x': (i/4)%canvas.width, 'y': Math.floor((i/4)/canvas.width) };
//                    }
//                } return null;";
//            return js.ExecuteScript(script, TargetRGB[0], TargetRGB[1], TargetRGB[2], tolerance) as Dictionary<string, object>;
//        }

//        static void TriggerPreciseClick(IWebDriver driver, Dictionary<string, object> coords)
//        {
//            var js = (IJavaScriptExecutor)driver;
//            string script = @"
//                var canvas = document.querySelector('canvas');
//                var rect = canvas.getBoundingClientRect();
//                var sX = rect.width / canvas.width;
//                var sY = rect.height / canvas.height;
//                var bX = arguments[0];
//                var bY = arguments[1];
//                var pts = [[0,0], [4,0], [0,4], [-4,0], [0,-4], [4,4], [-4,-4], [4,-4], [-4,4]];
//                pts.forEach(function(p) {
//                    var cX = rect.left + (bX + p[0]) * sX;
//                    var cY = rect.top + (bY + p[1]) * sY;
//                    ['pointerdown', 'mousedown', 'pointerup', 'mouseup', 'click'].forEach(t => {
//                        var cls = t.includes('pointer') ? PointerEvent : MouseEvent;
//                        canvas.dispatchEvent(new cls(t, { view: window, bubbles: true, clientX: cX, clientY: cY, buttons: 1 }));
//                    });
//                });";
//            js.ExecuteScript(script, coords["x"], coords["y"]);
//        }

//        static IWebElement FindMapIframe(IWebDriver driver)
//        {
//            for (int i = 0; i < 10; i++)
//            {
//                driver.SwitchTo().DefaultContent();
//                var frames = driver.FindElements(By.TagName("iframe"));
//                foreach (var f in frames)
//                {
//                    try
//                    {
//                        driver.SwitchTo().Frame(f);
//                        if (driver.FindElements(By.TagName("canvas")).Count > 0) return f;
//                    }
//                    catch { }
//                    driver.SwitchTo().DefaultContent();
//                }
//                Thread.Sleep(1000);
//            }
//            return null;
//        }
//    }
//}










// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace WeBookAutomation
{
    class Program
    {
        // --- CONFIGURATION ---
        const string EMAIL = "cabnipcar@bangban.uk";
        const string PASSWORD = "Aa@123456789";
        const string URL = "https://webook.com/en/events/rsl-al-khaleej-vs-al-hilal-387468/book";
        static readonly int[] TargetRGB = { 139, 195, 74 };

        static void Main(string[] args)
        {
            // 1. CLEANUP: Kill any lingering drivers from previous runs
            foreach (var p in Process.GetProcessesByName("chromedriver")) { try { p.Kill(); } catch { } }

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--force-device-scale-factor=1");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            // 2. INITIALIZATION: Open browser and set up explicit wait
            IWebDriver driver = new ChromeDriver(options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                Console.WriteLine("🚀 Workflow Started...");
                driver.Navigate().GoToUrl(URL);

                // --- 3. LOGIN FLOW ---
                HandleLogin(driver, wait);

                // --- 4. IFRAME DETECTION ---
                // The seat map is inside a nested frame; we must find it to interact with the canvas
                IWebElement mapIframe = FindMapIframe(driver);
                if (mapIframe == null) { Console.WriteLine("❌ Map Not Found."); return; }

                // --- 5. SELECTION STRATEGY ---

                // STEP A: Initial Section Zoom
                // We click once to zoom into the specific stadium section
                Console.WriteLine("🎯 Zooming to section...");
                var zoomCoords = GetSeatCoords(driver, 20);
                if (zoomCoords != null)
                {
                    TriggerPreciseClick(driver, zoomCoords);
                    // Wait for the 'Zoom' animation to finish so the seat pixels stabilize
                    Thread.Sleep(5000);
                }

                // STEP B: Precise Seat Selection Loop
                Console.WriteLine("💺 Selecting seat with Spiral Precision...");
                for (int attempt = 0; attempt < 40; attempt++)
                {
                    var seatCoords = GetSeatCoords(driver, 35);
                    if (seatCoords != null)
                    {
                        // Use the 9-point spiral click to ensure the hitbox is hit
                        TriggerPreciseClick(driver, seatCoords);
                        Thread.Sleep(1000);                                 ///////////////////////////////////

                        // 6. CHECKOUT VERIFICATION
                        // Switch back to the main page to see if the 'Checkout' button appeared
                        driver.SwitchTo().DefaultContent();

                        var checkoutBtn = driver.FindElements(By.XPath("//button[contains(., 'Next') or contains(., 'Checkout')]"))
                                                .FirstOrDefault(b => b.Enabled && b.Displayed);

                        if (checkoutBtn != null)
                        {
                            Console.WriteLine("🎉 SUCCESS: Seat Captured!");

                            // Perform the final click on the Checkout button
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", checkoutBtn);

                            // EXIT POINT: We have reached your goal.
                            break;
                        }

                        // If button not found, jump back into the frame for the next attempt
                        driver.SwitchTo().Frame(mapIframe);
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"❌ Runtime Error: {e.Message}");
            }

            // --- 7. THE 'STAY OPEN' MECHANISM ---
            Console.WriteLine("\n✅ Task Finished. Browser is now in manual mode.");
            Console.WriteLine("Please complete the payment in the opened window.");

            // This loop keeps the C# process alive so the browser window doesn't close
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        static void HandleLogin(IWebDriver driver, WebDriverWait wait)
        {
            try
            {
                var reject = wait.Until(d => d.FindElements(By.XPath("//button[contains(., 'Reject')]")).FirstOrDefault());
                reject?.Click();

                wait.Until(d => d.FindElement(By.CssSelector("input[data-testid='auth_login_email_input']"))).SendKeys(EMAIL);
                driver.FindElement(By.CssSelector("input[data-testid='auth_login_password_input']")).SendKeys(PASSWORD);
                driver.FindElement(By.CssSelector("button[data-testid='auth_login_submit_button']")).Click();
                Thread.Sleep(3000);
            }
            catch { Console.WriteLine("ℹ️ Login elements not found (perhaps already logged in)."); }
        }

        static Dictionary<string, object> GetSeatCoords(IWebDriver driver, int tolerance)
        {
            var js = (IJavaScriptExecutor)driver;
            string script = @"
                var canvas = document.querySelector('canvas');
                if (!canvas) return null;
                var ctx = canvas.getContext('2d', {willReadFrequently: true});
                var data = ctx.getImageData(0, 0, canvas.width, canvas.height).data;
                var tr = arguments[0], tg = arguments[1], tb = arguments[2], tol = arguments[3];
                for (var i = 0; i < data.length; i += 16) {
                    if (Math.abs(data[i]-tr)<tol && Math.abs(data[i+1]-tg)<tol && Math.abs(data[i+2]-tb)<tol) {
                        return { 'x': (i/4)%canvas.width, 'y': Math.floor((i/4)/canvas.width) };
                    }
                } return null;";
            return js.ExecuteScript(script, TargetRGB[0], TargetRGB[1], TargetRGB[2], tolerance) as Dictionary<string, object>;
        }

        static void TriggerPreciseClick(IWebDriver driver, Dictionary<string, object> coords)
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
                });";
            js.ExecuteScript(script, coords["x"], coords["y"]);
        }

        static IWebElement FindMapIframe(IWebDriver driver)
        {
            for (int i = 0; i < 10; i++)
            {
                driver.SwitchTo().DefaultContent();
                var frames = driver.FindElements(By.TagName("iframe"));
                foreach (var f in frames)
                {
                    try
                    {
                        driver.SwitchTo().Frame(f);
                        if (driver.FindElements(By.TagName("canvas")).Count > 0) return f;
                    }
                    catch { }
                    driver.SwitchTo().DefaultContent();
                }
                Thread.Sleep(1000);
            }
            return null;
        }
    }
}