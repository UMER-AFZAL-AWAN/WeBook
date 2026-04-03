using System;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using WeBook.Utilities;

namespace WeBook.Engines
{
    public static class SeatEngine
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static bool Reserve(IWebDriver driver, string sectionLabel, int quantity)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Actions action = new Actions(driver);

            Logger.Log($"Initiating automated reservation for {quantity} seats in {sectionLabel}...");

            // --- STEP 1: COLLAPSE SIDEBAR ---
            try
            {
                var categoryTab = driver.FindElements(By.CssSelector("button[class*='z-50']")).FirstOrDefault();
                if (categoryTab != null)
                {
                    action.MoveToElement(categoryTab).Click().Build().Perform();
                    Logger.Log("Clicked Category Tab. Sidebar collapsing...");
                }
                Thread.Sleep(3000);
            }
            catch { }

            // --- STEP 2: ACTIVE SECURITY SOLVER & MAP SYNC ---
            bool mapFound = false;
            Logger.Log("Monitoring security clearance. Hunting for blockers...");

            for (int i = 1; i <= 15; i++)
            {
                // 1. Check for the SeatCloud iframe (The Goal)
                var mapFrames = driver.FindElements(By.CssSelector("iframe[id*='seat-cloud'], iframe[src*='seatcloud']"));
                if (mapFrames.Count > 0 && mapFrames[0].Displayed)
                {
                    Logger.Log("✅ Map detected! Proceeding to reservation...");
                    mapFound = true;
                    break;
                }

                // 2. Check for Cloudflare Turnstile (The Blocker)
                var cfFrames = driver.FindElements(By.CssSelector("iframe[title*='Cloudflare'], iframe[src*='turnstile']"));
                if (cfFrames.Count > 0)
                {
                    Logger.Log($"⚠️ Cloudflare detected (Attempt {i}). Attempting manual bypass...");
                    try
                    {
                        driver.SwitchTo().Frame(cfFrames[0]);
                        // Target the checkbox specifically
                        var checkbox = driver.FindElements(By.CssSelector("input[type='checkbox'], #challenge-stage, .cb-i")).FirstOrDefault();
                        if (checkbox != null)
                        {
                            js.ExecuteScript("arguments[0].click();", checkbox);
                            Logger.Log("🖱️ Security checkbox clicked.");
                        }
                        driver.SwitchTo().DefaultContent();
                    }
                    catch { driver.SwitchTo().DefaultContent(); }
                }

                // 3. Heartbeat: Nudge and Ensure Sidebar is closed
                if (i % 3 == 0)
                {
                    Logger.Log("Nudging page and checking sidebar state...");
                    // Ensure sidebar didn't pop back up
                    var sectionList = driver.FindElements(By.CssSelector("div[class*='overflow-y-auto']")).FirstOrDefault();
                    if (sectionList != null && sectionList.Displayed)
                    {
                        var categoryTab = driver.FindElements(By.CssSelector("button[class*='z-50']")).FirstOrDefault();
                        if (categoryTab != null) js.ExecuteScript("arguments[0].click();", categoryTab);
                    }
                    js.ExecuteScript("window.scrollTo(0, 150);");
                    // Click far right side to avoid sidebar
                    try { action.MoveToLocation(driver.Manage().Window.Size.Width - 50, 300).Click().Build().Perform(); } catch { }
                }

                Logger.Log($"Waiting... (Attempt {i}/15)");
                Thread.Sleep(4000);
            }

            if (!mapFound) return false;

            // --- STEP 3: INTERACT WITH MAP ---
            try
            {
                var frames = driver.FindElements(By.CssSelector("iframe[id*='seat-cloud'], iframe[src*='seatcloud']"));
                if (frames.Count == 0) return false;

                driver.SwitchTo().Frame(frames[0]);

                var canvas = driver.FindElements(By.TagName("canvas")).FirstOrDefault();
                if (canvas != null)
                {
                    js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", canvas);
                    Thread.Sleep(2000);

                    // Double-click to focus and select
                    action.MoveToElement(canvas).Click().Click().Build().Perform();

                    Logger.Log("✅ Map clicked. Waiting for quantity popup to stabilize...");
                    Thread.Sleep(4000);

                    // --- STEP 4: QUANTITY & CHECKOUT ---
                    string bookingScript = @"
                        let plus = document.getElementById('ga-increase-seats');
                        let confirm = document.getElementById('ga-confirm-seats');
                        if (confirm && confirm.offsetWidth > 0) {
                            for(let i = 1; i < arguments[0]; i++) { if(plus) plus.click(); }
                            confirm.click();
                            return true;
                        }
                        return false;";

                    bool bookSuccess = false;
                    for (int i = 0; i < 3; i++)
                    {
                        var result = js.ExecuteScript(bookingScript, quantity);
                        if (result is bool success && success)
                        {
                            bookSuccess = true;
                            break;
                        }
                        Thread.Sleep(2000);
                    }

                    if (bookSuccess)
                    {
                        driver.SwitchTo().DefaultContent();
                        Logger.Log("Quantity confirmed. Looking for final Checkout button...");
                        Thread.Sleep(2500);

                        var checkout = driver.FindElements(By.XPath("//button[contains(@class, 'bg-primary') or contains(., 'Checkout')]")).FirstOrDefault();
                        if (checkout != null)
                        {
                            js.ExecuteScript("arguments[0].click();", checkout);
                            Logger.Log("🚀 Success! Redirecting to payment.");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Interaction Error: {ex.Message}");
            }
            finally
            {
                driver.SwitchTo().DefaultContent();
            }

            return false;
        }

        public static void BringConsoleToFront()
        {
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                SetForegroundWindow(handle);
            }
        }
    }
}