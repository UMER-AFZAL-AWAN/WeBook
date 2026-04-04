//using System;
//using System.Threading;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Interactions;
//using OpenQA.Selenium.Support.UI;
//using WeBook.Utilities;

//namespace WeBook.Engines
//{
//    public static class SeatEngine
//    {
//        public static void Reserve(IWebDriver driver, string sectionLabel, int quantity)
//        {
//            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
//            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
//            Actions action = new Actions(driver);

//            // --- NEW: MID-PROCESS SECURITY RE-CHECK ---
//            Logger.Log("Checking for post-selection security challenges...");
//            try
//            {
//                // Look for the specific 'Turnstile' or Cloudflare container
//                var securityCheck = driver.FindElements(By.CssSelector("iframe[src*='cloudflare'], .cf-turnstile, #turnstile-wrapper"));
//                if (securityCheck.Count > 0)
//                {
//                    Logger.Log("⚠️ Cloudflare/Turnstile detected again. Waiting 8s for auto-resolve...");
//                    Thread.Sleep(8000);
//                    // If it's a click-based checkbox, we might need to click the center of that frame
//                    // driver.SwitchTo().Frame(securityCheck[0]);
//                    // action.Click().Build().Perform();
//                    // driver.SwitchTo().DefaultContent();
//                }
//            }
//            catch { /* Ignore if not found */ }

//            // --- STEP 1: FORCE CLOSE THE SECTION LIST OVERLAY ---
//            try
//            {
//                js.ExecuteScript("let overlay = document.querySelector('div[data-open=\"true\"]'); if(overlay) overlay.style.display = 'none';");
//                var body = driver.FindElement(By.TagName("body"));
//                action.MoveToElement(body, 0, 0).Click().Build().Perform();
//                Logger.Log("Cleared section list overlay.");
//                Thread.Sleep(1000); // Give the DOM a second to stabilize
//            }
//            catch (Exception ex) { Logger.Log("Note: Overlay clear skipped: " + ex.Message); }

//            // --- STEP 2: SMART CANVAS DETECTION ---
//            bool canvasFound = false;
//            IWebElement targetCanvas = null;

//            // Retry loop for the canvas (in case Cloudflare is still loading)
//            for (int i = 0; i < 3; i++)
//            {
//                var mainCanvases = driver.FindElements(By.TagName("canvas"));
//                if (mainCanvases.Count > 0)
//                {
//                    targetCanvas = mainCanvases[0];
//                    canvasFound = true;
//                    break;
//                }

//                var frames = driver.FindElements(By.TagName("iframe"));
//                foreach (var frame in frames)
//                {
//                    try
//                    {
//                        driver.SwitchTo().Frame(frame);
//                        var frameCanvases = driver.FindElements(By.TagName("canvas"));
//                        if (frameCanvases.Count > 0)
//                        {
//                            targetCanvas = frameCanvases[0];
//                            canvasFound = true;
//                            break;
//                        }
//                        driver.SwitchTo().DefaultContent();
//                    }
//                    catch { driver.SwitchTo().DefaultContent(); }
//                }

//                if (canvasFound) break;
//                Logger.Log($"Retry {i + 1}: Canvas not found yet. Map might still be loading behind security...");
//                Thread.Sleep(3000);
//            }

//            if (!canvasFound || targetCanvas == null)
//            {
//                Logger.Log("❌ Critical: Could not find canvas. Security block likely active.");
//                return;
//            }

//            // --- STEP 3: INTERACT WITH CANVAS ---
//            try
//            {
//                // Scroll to the canvas to make sure it's in view
//                js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", targetCanvas);
//                Thread.Sleep(1000); // Wait for scroll to settle

//                // Click the center of the canvas to trigger the "Seat Selection" popup
//                action.MoveToElement(targetCanvas).Click().Build().Perform();
//                Logger.Log($"✅ Canvas clicked for section: {sectionLabel}. Waiting for popup...");

//                // Wait for the WeBook selection popup to render
//                Thread.Sleep(2000);
//            }
//            catch (Exception ex)
//            {
//                Logger.Log($"❌ Click interaction failed: {ex.Message}");
//                return;
//            }

//            // --- STEP 4: SELECTION UI AUTOMATION (JavaScript) ---
//            // We use JS here because the popup buttons are often inside complex shadow DOMs 
//            // or high-index layers that standard Selenium clicks struggle with.
//            try
//            {
//                string script = @"
//                    let confirmBtn = document.getElementById('ga-confirm-seats');
//                    let plusBtn = document.getElementById('ga-increase-seats');
                    
//                    if (confirmBtn) {
//                        let targetQty = arguments[0];
//                        // Click plus button (targetQty - 1) times because it starts at 1
//                        for(let i = 1; i < targetQty; i++) {
//                            if(plusBtn) plusBtn.click();
//                        }
                        
//                        confirmBtn.click();
//                        return true;
//                    }
//                    return false;";

//                bool success = (bool)js.ExecuteScript(script, quantity);

//                if (success)
//                {
//                    Logger.Log($"✅ SUCCESS: {quantity} seats added for {sectionLabel}. Proceeding to checkout...");
//                }
//                else
//                {
//                    Logger.Log("⚠️ Warning: Could not find confirmation buttons. You may need to click the specific section on the map manually.");
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log($"❌ Script execution failed: {ex.Message}");
//            }
//        }
//    }
//}