using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WeBook.Helpers;

namespace WeBook.Services
{
    public class SeatSelectionService
    {
        private readonly IWebDriver _driver;
        private readonly ColorDetector _colorDetector;
        private readonly List<Tuple<int, int>> _clickedPositions = new List<Tuple<int, int>>();

        public SeatSelectionService(IWebDriver driver)
        {
            _driver = driver;
            _colorDetector = new ColorDetector(driver);
        }
        public bool ClickSection(int r, int g, int b)
        {
            Logger.Debug("Looking for section to click...");
            var result = _colorDetector.FindColorCoordinate(r, g, b, isSeatMode: false);

            if (!result.Success)
            {
                Logger.Debug($"Section click failed: {result.Error}");
                return false;
            }

            // Get the canvas element (assumes we are in the correct frame)
            var canvas = _driver.FindElement(By.Id("canvas"));

            // Use Actions to click at the relative coordinates
            var actions = new Actions(_driver);
            actions.MoveToElement(canvas, result.X, result.Y)
                   .Click()
                   .Perform();

            Logger.Click($"Section clicked at X:{result.X} Y:{result.Y}");
            return true;
        }

        public bool ClickSeat(int r, int g, int b)
        {
            Logger.Debug("Looking for seat to click...");
            var result = _colorDetector.FindColorCoordinate(r, g, b, isSeatMode: true);

            if (!result.Success)
            {
                Logger.Debug($"Seat click failed: {result.Error}");
                return false;
            }

            // Check for duplicates using internal coordinates
            bool alreadyClicked = false;
            foreach (var pos in _clickedPositions)
            {
                if (Math.Abs(pos.Item1 - result.X) < 20 && Math.Abs(pos.Item2 - result.Y) < 20)
                {
                    alreadyClicked = true;
                    break;
                }
            }

            if (alreadyClicked)
            {
                Logger.Warning($"Skipping duplicate seat at X:{result.X} Y:{result.Y}");
                return false;
            }

            _clickedPositions.Add(Tuple.Create(result.X, result.Y));

            // Get the canvas element
            var canvas = _driver.FindElement(By.Id("canvas"));

            // Focus the canvas
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].focus();", canvas);

            // Use viewport coordinates for the click
            var actions = new Actions(_driver);
            actions.MoveToElement(canvas, (int)result.ViewportX, (int)result.ViewportY)
                   .Click()
                   .Perform();

            Logger.Click($"Seat clicked at internal ({result.X},{result.Y}) -> viewport ({result.ViewportX:F1},{result.ViewportY:F1})");
            return true;
        }
        public bool HandleQuantityPopup(int remainingSeats)
        {
            try
            {
                Logger.Debug("Checking for quantity popup...");

                var popup = _driver.FindElements(By.Id("ga-popup")).FirstOrDefault();
                if (popup == null || !popup.Displayed)
                {
                    Logger.Debug("No popup found");
                    return false;
                }

                Logger.Success("Quantity popup detected!");

                // Get seat count input
                var seatCountInput = _driver.FindElement(By.Id("ga-seat-count"));
                string currentValue = seatCountInput.GetAttribute("value");
                int currentSeats = int.Parse(currentValue);

                // Calculate seats to select (max 4 per selection)
                int seatsToSelect = Math.Min(remainingSeats, 4);

                Logger.Debug($"Current quantity: {currentSeats}, Setting to: {seatsToSelect}");

                // Set the quantity
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    $"arguments[0].value = '{seatsToSelect}'; " +
                    "arguments[0].dispatchEvent(new Event('input', { bubbles: true })); " +
                    "arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                    seatCountInput);

                Thread.Sleep(500);

                // Click confirm button
                var confirmBtn = _driver.FindElement(By.Id("ga-confirm-seats"));
                if (confirmBtn.Displayed && confirmBtn.Enabled)
                {
                    Logger.Click($"Confirming {seatsToSelect} seat(s)");
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", confirmBtn);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Popup handling error: {ex.Message}");
                return false;
            }
        }

        public bool ProceedToCheckout()
        {
            try
            {
                Logger.Step("Looking for checkout button...");
                Thread.Sleep(2000);

                var checkout = _driver.FindElements(By.XPath("//button[contains(., 'Checkout') or contains(., 'Next')]"))
                    .FirstOrDefault(e => e.Displayed && e.Enabled);

                if (checkout != null)
                {
                    Logger.Click("Clicking checkout button");
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", checkout);
                    Thread.Sleep(3000);
                    return true;
                }
                else
                {
                    Logger.Warning("Checkout button not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Checkout error: {ex.Message}");
                return false;
            }
        }

        public void ResetClickedPositions()
        {
            _clickedPositions.Clear();
            Logger.Debug("Reset clicked positions tracking");
        }

        public int GetClickedCount()
        {
            return _clickedPositions.Count;
        }
    }
}