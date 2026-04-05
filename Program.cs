using System;
using OpenQA.Selenium;
using WeBook.Core;
using WeBook.Services;
using WeBook.Utilities;
using WeBook.Models;
using WeBook.Engines;
using WeBook.UI;

class Program
{
    static void Main()
    {
        // 1. SETUP: Initialize user data and credentials
        var request = ConsoleTerminal.InitializeRequest();
        var myCredentials = new UserCredentials();

        // Validate URL input before launching browser
        if (string.IsNullOrEmpty(request.TargetUrl))
        {
            Logger.Log("❌ Error: No URL provided. Closing...");
            return;
        }

        // 2. BROWSER START: Initialize the Selenium Driver
        IWebDriver driver = DriverFactory.Create();

        try
        {
            // 3. NAVIGATION: Load the event page once
            Navigator.GoToEvent(driver, request.TargetUrl);

            // 4. OBSTACLES: Handle cookie banners to ensure the UI is interactable
            BookingEngine.HandleCookieBanner(driver);

            // 5. LOGIN: Execute credentials injection and wait for map verification
            // This instance call handles the "SendKeys" logic and redirect verification.
            var auth = new AuthenticationService();
            auth.LoginAndSolve(driver, myCredentials);

            // 6. SCRAPE & COUNT: Discover available sections (A1, G4, etc.)
            // We ensure the 'Enclosure' tab is open before scanning the HTML.
            Logger.Log("Waiting for stadium map elements...");
            DiscoveryService.EnsureEnclosureTabOpen(driver);

            var availableSections = DiscoveryService.ScanAvailableSections(driver);
            Logger.Log($"✅ Found {availableSections.Count} available sections.");

            // 7. USER SELECTION: Presents the user with a numbered list of section labels
            // This displays options like [0] A1, [1] B3, etc.
            var selectedSection = ConsoleTerminal.UserPickSection(availableSections);

            if (selectedSection != null)
            {
                // 8. PRICE REVEAL: Display the specific price for the user's choice
                Logger.Log($"🎯 You selected {selectedSection.Section}. Price is {selectedSection.Price} SAR.");

                // 9. MAP SEARCH: Switch to the map view to locate and click the selected section
                Logger.Log($"🔍 Locating {selectedSection.Section} on the stadium map...");
                bool mapSuccess = MapInteractionEngine.SelectSectionOnMap(driver, selectedSection);

                // 10. RESERVATION: Select specific seats and proceed to checkout
                if (mapSuccess)
                {
                    BookingEngine.FinalizeReservation(driver, request.Quantity);
                }
                else
                {
                    Logger.Log("❌ Failed: Could not find the section on the map.");
                }
            }
        }
        catch (Exception ex)
        {
            // Catch-all for automation errors (e.g., timeout, element not found)
            Logger.Log($"Critical Error: {ex.Message}");
        }
        finally
        {
            // Ensures the terminal stays open to view logs
            ConsoleTerminal.KeepAlive();
        }
    }
}