using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using WeBook.Utilities;

namespace WeBook.Core
{
    public static class InteractionHelper
    {
        private static IWebDriver _driver;
        private static IJavaScriptExecutor _js;
        private static Actions _actions;

        public static void Initialize(IWebDriver driver)
        {
            _driver = driver;
            _js = (IJavaScriptExecutor)driver;
            _actions = new Actions(driver);
        }

        // Click with JavaScript fallback
        public static void Click(IWebElement element, bool useJS = false)
        {
            if (useJS)
                _js.ExecuteScript("arguments[0].click();", element);
            else
                element.Click();

            Thread.Sleep(300); // brief pause
            CloudflareGuard.Check(_driver); // after every click
        }

        // Click by coordinates relative to element
        public static void ClickAtCoordinates(IWebElement element, int offsetX, int offsetY)
        {
            _actions.MoveToElement(element, offsetX, offsetY).Click().Perform();
            Thread.Sleep(300);
            CloudflareGuard.Check(_driver);
        }

        // Click center of element
        public static void ClickCenter(IWebElement element)
        {
            _actions.MoveToElement(element).Click().Perform();
            Thread.Sleep(300);
            CloudflareGuard.Check(_driver);
        }

        // Drag and drop
        public static void DragAndDrop(IWebElement source, IWebElement target)
        {
            _actions.DragAndDrop(source, target).Perform();
            Thread.Sleep(500);
            CloudflareGuard.Check(_driver);
        }

        // Get element coordinates
        public static (int x, int y) GetCoordinates(IWebElement element)
        {
            var location = element.Location;
            return (location.X, location.Y);
        }

        // Scroll element into view
        public static void ScrollToElement(IWebElement element, bool center = true)
        {
            string block = center ? "center" : "start";
            _js.ExecuteScript($"arguments[0].scrollIntoView({{block: '{block}'}});", element);
            Thread.Sleep(500);
            CloudflareGuard.Check(_driver);
        }

        // Scroll container by pixels
        public static void ScrollContainerBy(IWebElement container, int pixels)
        {
            _js.ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", container, pixels);
            Thread.Sleep(300);
            CloudflareGuard.Check(_driver);
        }

        // Focus canvas or any element
        public static void FocusElement(IWebElement element)
        {
            _js.ExecuteScript("arguments[0].focus();", element);
            CloudflareGuard.Check(_driver);
        }

        // Hide overlay by CSS selector
        public static void HideOverlay(string cssSelector)
        {
            _js.ExecuteScript($"let el = document.querySelector('{cssSelector}'); if(el) el.style.display = 'none';");
            CloudflareGuard.Check(_driver);
        }

        // Execute custom JS and return result
        public static object ExecuteScript(string script, params object[] args)
        {
            var result = _js.ExecuteScript(script, args);
            CloudflareGuard.Check(_driver);
            return result;
        }
    }
}