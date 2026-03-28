using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;
using WeBook.Helpers;

namespace WeBook.Services
{
    public class FrameManager
    {
        private readonly IWebDriver _driver;
        private IWebElement _currentMapFrame;
        private readonly WebDriverWait _wait;

        public FrameManager(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        }
        public void FocusCanvas()
        {
            var canvas = _driver.FindElement(By.Id("canvas"));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].focus();", canvas);
        }
        public bool FindMapFrame(int maxAttempts = 30)
        {
            Logger.Debug("Waiting for page to fully load...");

            // First, wait for the page to be fully loaded
            try
            {
                _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
                Logger.Success("Page fully loaded");
            }
            catch
            {
                Logger.Warning("Page load timeout, continuing anyway...");
            }

            // Wait for any iframes to appear
            Logger.Debug("Waiting for iframes to load...");
            try
            {
                _wait.Until(d => d.FindElements(By.TagName("iframe")).Count > 0);
                Logger.Success("Iframes detected");
            }
            catch
            {
                Logger.Warning("No iframes found after waiting");
            }

            // Additional wait for canvas to be ready
            Thread.Sleep(2000);

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                _driver.SwitchTo().DefaultContent();
                var iframes = _driver.FindElements(By.TagName("iframe"));

                Logger.Debug($"Checking {iframes.Count} iframes (attempt {attempt + 1}/{maxAttempts})");

                foreach (var frame in iframes)
                {
                    try
                    {
                        _driver.SwitchTo().Frame(frame);

                        // Wait for canvas to be present and ready
                        try
                        {
                            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                            var canvasElement = wait.Until(d =>
                            {
                                var c = d.FindElement(By.Id("canvas"));
                                return c.Displayed && c.Enabled ? c : null;
                            });

                            if (canvasElement != null && canvasElement.Displayed)
                            {
                                Logger.Success($"Found map canvas in iframe on attempt {attempt + 1}");
                                _currentMapFrame = frame;
                                return true;
                            }
                        }
                        catch (WebDriverTimeoutException)
                        {
                            Logger.Debug("Canvas not ready in this iframe");
                        }
                        catch (NoSuchElementException)
                        {
                            Logger.Debug("No canvas element in this iframe");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Error switching to frame: {ex.Message}");
                    }
                    finally
                    {
                        if (_currentMapFrame == null)
                            _driver.SwitchTo().DefaultContent();
                    }
                }

                // Wait before next attempt
                Thread.Sleep(1000);
            }

            Logger.Error("Could not find map iframe");
            return false;
        }

        public void EnterMapFrame()
        {
            if (_currentMapFrame != null)
            {
                try
                {
                    _driver.SwitchTo().DefaultContent();
                    _driver.SwitchTo().Frame(_currentMapFrame);

                    // Wait a moment for frame to be ready
                    Thread.Sleep(500);
                }
                catch
                {
                    Logger.Warning("Frame reference stale, refinding...");
                    FindMapFrame();
                    if (_currentMapFrame != null)
                        _driver.SwitchTo().Frame(_currentMapFrame);
                }
            }
            else
            {
                FindMapFrame();
            }
        }

        public void ExitToMainContent()
        {
            _driver.SwitchTo().DefaultContent();
        }

        public bool WaitForElement(By by, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(d => d.FindElement(by));
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool WaitForPopup(string popupId, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                var popup = wait.Until(d =>
                {
                    var element = d.FindElement(By.Id(popupId));
                    if (element.Displayed && element.GetAttribute("style").Contains("visible"))
                        return element;
                    return null;
                });
                return popup != null;
            }
            catch
            {
                return false;
            }
        }

        public bool IsCanvasReady()
        {
            try
            {
                var canvas = _driver.FindElement(By.Id("canvas"));
                return canvas.Displayed && canvas.Enabled;
            }
            catch
            {
                return false;
            }
        }
    }
}