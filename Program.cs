using System;
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
        InteractionHelper.Initialize(driver);

        try
        {
            Navigator.GoToEvent(driver, request.TargetUrl);
            CloudflareGuard.Check(driver);

            BookingService.HandleCookieBanner(driver);
            AuthService.Login(driver, wait);
            CloudflareGuard.Check(driver);

            BookingService.ConfirmTeamSelection(driver);

            Logger.Log("Waiting for stadium map elements...");
            wait.Until(d => d.FindElements(By.CssSelector("button[class*='z-50']")).Count > 0);

            CategorySelector.OpenEnclosureTab(driver);
            var availableSeats = CategorySelector.GetAllEnclosures(driver);
            Logger.Log($"✅ Discovery complete. {availableSeats.Count} sections mapped.");

            Console.Write("\n🎯 Enter Section Label (e.g., S7, B3, G12): ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;
            var target = availableSeats.FirstOrDefault(s =>
                s.Section.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                Logger.Log($"🎯 Target Match: {target.Section} Price: {target.Price} SAR.");
                // Use GA Popup handler (you can switch to generic if needed)
                StadiumMapHandler.ReserveWithGaPopup(driver, target.Section, request.Quantity);
                Logger.Log("SUCCESS: Seats added to cart. Proceeding to checkout...");
                BookingService.ClickCheckout(driver);
            }
            else
            {
                Logger.Log($"❌ Section '{input}' not found.");
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