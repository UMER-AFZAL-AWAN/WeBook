using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V143.Network;
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

            /// --- Team selection (if needed) ---
            if (TeamSelectionScreenService.IsTeamSelectionScreen(driver))
                TeamSelectionScreenService.HandleTeamSelection(driver);



            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // --- Fetch categories from main page ---
            Console.WriteLine("\nFetching available categories...");
            var categories = StadiumScreenCategoryService.GetCategories(driver);
            if (categories.Count == 0)
            {
                Console.WriteLine("❌ No categories found.");
                return;
            }

            Console.WriteLine("\nAvailable categories:");
            for (int i = 0; i < categories.Count; i++)
                Console.WriteLine($"{i + 1}. {categories[i].Name}");

            Console.Write("\nSelect category number: ");
            int selectedIndex = int.Parse(Console.ReadLine()) - 1;
            if (selectedIndex < 0 || selectedIndex >= categories.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
            var selectedCategory = categories[selectedIndex];
            Console.WriteLine($"Selected: {selectedCategory.Name}");

            Console.Write("Number of seats: ");
            int seatCount = int.Parse(Console.ReadLine());
            seatCount = Math.Clamp(seatCount, 1, 5);

            Console.Write("Require consecutive seats? (y/n): ");
            bool requireConsecutive = Console.ReadLine().ToLower() == "y";

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




            //Logger.Log("Waiting for stadium map elements...");
            //wait.Until(d => d.FindElements(By.CssSelector("button[class*='z-50']")).Count > 0);

            //// 4. SCAN - The new service handles the "12-item" bottleneck automatically
            //DiscoveryService.EnsureEnclosureTabOpen(driver);
            //var availableSeats = DiscoveryService.GetAllEnclosures(driver);

            //Logger.Log($"✅ Discovery complete. {availableSeats.Count} total sections mapped across all categories.");

            //// 5. USER SELECTION
            //Console.Write("\n?? Enter Section Label (e.g., S7, B3, G12): ");
            //string input = Console.ReadLine()?.Trim() ?? string.Empty;

            //var target = availableSeats.FirstOrDefault(s =>
            //    s.Section.Equals(input, StringComparison.OrdinalIgnoreCase));

            // Inside your Main method in Program.cs
            //    if (target != null)
            //    {
            //        Logger.Log($"🎯 Target Match: {target.Section} Found. Price: {target.Price} SAR.");

            //        // Call the Engine and capture the true/false result
            //        bool isReserved = SeatEngine.Reserve(driver, target.Section, request.Quantity);

            //        if (isReserved)
            //        {
            //            Logger.Log("✅ SUCCESS: Seats added to cart. Proceeding to checkout...");
            //        }
            //        else
            //        {
            //            // This will trigger if the Canvas wasn't found or the popup didn't appear
            //            Logger.Log("❌ Reservation failed: Could not interact with the stadium map.");
            //        }
            //    }
        }
        catch (Exception ex)
        {
            Logger.Log($"Critical Failure: {ex.Message}");
        }

        Console.WriteLine("\n[System] Process finished. Browser will remain open for inspection.");
        while (true) Thread.Sleep(10000);
    }
}