using System;
using System.Collections.Generic;
using System.Linq;
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
        var request = new SeatRequest();

        request.TargetUrl = ConsoleInterface.GetUrl() ?? string.Empty;
        request.Quantity = ConsoleInterface.GetSeatCount();

        IWebDriver driver = DriverFactory.Create();
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));

        try
        {
            Navigator.GoToEvent(driver, request.TargetUrl);
            BookingService.HandleCookieBanner(driver);
            AuthService.Login(driver, wait);
            AuthService.SolveCloudflare(driver);

            Logger.Log("Waiting for stadium map elements...");
            wait.Until(d => d.FindElements(By.CssSelector("button[class*='z-50']")).Count > 0);

            // 4. SCAN - The new service handles the "12-item" bottleneck automatically
            DiscoveryService.EnsureEnclosureTabOpen(driver);
            var availableSeats = DiscoveryService.GetAllEnclosures(driver);

            Logger.Log($"✅ Discovery complete. {availableSeats.Count} total sections mapped across all categories.");

            // 5. USER SELECTION
            Console.Write("\n?? Enter Section Label (e.g., S7, B3, G12): ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            var target = availableSeats.FirstOrDefault(s =>
                s.Section.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                Logger.Log($"🎯 Target Match: {target.Section} Found. Price: {target.Price} SAR.");

                // 1. Force focus out of the list and onto the stadium
                // Close the sidebar to prepare for clicking the map
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("document.querySelector('canvas')?.focus();");
                    js.ExecuteScript("window.scrollTo(0, document.getElementById('canvas').offsetTop - 100);");
                    Thread.Sleep(1000);
                }
                catch { }

                Logger.Log($"Initiating automated reservation for {request.Quantity} seats...");
                // SeatEngine.Reserve(driver, target, request.Quantity);

                // 2. Call the Engine
                SeatEngine.Reserve(driver, target.Section, request.Quantity);

                Logger.Log("SUCCESS: Seats added to cart. Proceeding to checkout...");
            }
            else
            {
                Logger.Log($"❌ Section '{input}' was not found. Please check the spelling (e.g., S7 vs s7).");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Critical Failure: {ex.Message}");
        }

        Console.WriteLine("\n[System] Process finished. Browser will remain open for inspection.");
        while (true) Thread.Sleep(10000);
    }
}