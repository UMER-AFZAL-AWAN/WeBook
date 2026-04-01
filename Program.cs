using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WeBook.Core;
using WeBook.Engines;
using WeBook.Models;
using WeBook.Services;
using WeBook.UI;
using WeBook.Utilities;

class Program
{
    static void Main()
    {
        var request = new SeatRequest
        {
            TargetUrl = string.Empty,
            Quantity = 1
        };

        request.TargetUrl = ConsoleInterface.GetUrl() ?? string.Empty;

        IWebDriver driver = DriverFactory.Create();
        
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));

        try
        {
            // 1. Go to the site
            Navigator.GoToEvent(driver, request.TargetUrl);

            // 2. CRITICAL: Clear the cookie wall BEFORE doing anything else
            BookingService.HandleCookieBanner(driver);

            // This uses the Config.EMAIL and Config.PASSWORD we fixed earlier
            AuthService.Login(driver, wait);

            // Check if Cloudflare is blocking the view
            if (driver.PageSource.Contains("cloudflare") || driver.PageSource.Contains("Verify you are human"))
            {
                AuthService.SolveCloudflare(driver);
            }

            // 2. NEW GATEKEEPER: Wait for the stadium UI element instead of the URL
            Logger.Log("Waiting for stadium map elements to render...");

            try
            {
                // Wait up to 60 seconds for the toggle button to appear
                wait.Until(d => d.FindElements(By.CssSelector("button[class*='z-50'][class*='flex']")).Count > 0);
                Logger.Log("✅ Stadium map detected!");
            }
            catch (WebDriverTimeoutException)
            {
                Logger.Log("❌ Timeout: Stadium elements not found. Please check the browser.");
                throw; // Stop the script if we can't find the map
            }

            // Extraction
            // 3. Now that the wall is gone, open the enclosure tab
            DiscoveryService.EnsureEnclosureTabOpen(driver);

            // 4. Get available sections and log them
            // Trigger the deep-scan
            var availableSeats = DiscoveryService.GetAllEnclosures(driver);

            // NEW: Mandatory pause to let the browser's JavaScript engine catch up 
            // after processing 200+ DOM elements.
            Thread.Sleep(3000);

            Logger.Log($"Scan complete. {availableSeats.Count} items in memory. Ready for next step.");

            if (availableSeats.Count > 0)
            {
                foreach (var seat in availableSeats)
                {
                    Logger.Log($"[LISTED] {seat.Section} - {seat.Price}");
                }
            }
            else
            {
                Logger.Log("❌ Failed to capture enclosure list.");
            }

            // 5. Handle any team selection popups if they exist
            BookingService.ConfirmTeamSelection(driver);

            request.Quantity = ConsoleInterface.GetSeatCount();

            Logger.Log("Waiting for user to click a section on the stadium map...");

            bool popupFound = false;
            for (int i = 0; i < 60; i++)
            {
                var popups = driver.FindElements(By.Id("ga-popup"));
                if (popups.Count > 0 && popups[0].Displayed)
                {
                    popupFound = true;
                    break;
                }
                Thread.Sleep(1000);
            }

            if (popupFound)
            {
                PopupEngine.HandleQuantityPopup(driver, request.Quantity);
                Logger.Log("🎉 Seat selection sequence completed.");
            }
            else
            {
                Logger.Log("❌ Timeout: Stadium map popup did not appear.");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Critical Failure: {ex.Message}");
        }

        Console.WriteLine("\n[System] Process finished. Browser will remain open.");
        while (true) Thread.Sleep(10000);
    }
}