using System;
using System.Collections.Generic;
using System.Linq;
using WeBook.Models;
using WeBook.Utilities;

namespace WeBook.UI
{
    public static class ConsoleTerminal
    {
        public static UserRequest InitializeRequest()
        {
            var request = new UserRequest();
            Console.Write("?? Enter Event URL: ");
            request.TargetUrl = Console.ReadLine() ?? "";
            Console.Write("?? How many seats? (Select 1 to 5): ");
            if (int.TryParse(Console.ReadLine(), out int count))
            {
                request.Quantity = count;
            }

            return request;
        }

        public static SeatSelection? UserPickSection(List<SeatSelection> sections)
        {
            Console.WriteLine("\n--- Available Sections Scraped from HTML ---");

            // CASE 1: Scraper failed or map is still loading
            if (sections == null || sections.Count == 0)
            {
                Logger.Log("⚠️ Warning: No sections were detected automatically.");
                Console.Write("?? Enter Section Label manually (e.g., A1, G4): ");
                string manualLabel = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(manualLabel)) return null;

                return new SeatSelection { Section = manualLabel, Price = "Unknown" };
            }

            // CASE 2: Success! Show the numbered "Quick Pick" menu
            for (int i = 0; i < sections.Count; i++)
            {
                Console.WriteLine($"[{i}] Section: {sections[i].Section} | Price: {sections[i].Price} SAR");
            }

            Console.Write("\n?? Select Index Number (or type the Label): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            // 1. Check if user typed the number (index)
            if (int.TryParse(input, out int index) && index >= 0 && index < sections.Count)
            {
                return sections[index];
            }

            // 2. Fallback: Check if user typed the string label instead
            var matchedByLabel = sections.FirstOrDefault(x => x.Section.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (matchedByLabel != null) return matchedByLabel;

            Logger.Log("❌ Invalid selection.");
            return null;
        }

        public static void KeepAlive()
        {
            Console.WriteLine("\n[System] Process finished. Press any key to exit.");
            Console.ReadKey();
        }
    }
}