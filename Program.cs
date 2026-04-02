using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using WeBook;
using WeBook.Helper;

namespace WeBookAutomation
{
    class Program
    {
        const string EMAIL = "cabnipcar@bangban.uk";
        const string PASSWORD = "Aa@123456789";
        const string URL = "https://webook.com/en/events/rsl-25-26-neom-vs-al-al-fayah-04042026/book";
        static readonly int DEFAULT_TOLERANCE = 15;
        static readonly int CLUSTER_DISTANCE = 12;

        static void Main(string[] args)
        {
            foreach (var p in Process.GetProcessesByName("chromedriver")) { try { p.Kill(); } catch { } }

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--force-device-scale-factor=1");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            IWebDriver driver = new ChromeDriver(options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            try
            {
                Console.WriteLine("🚀 Workflow Started...");
                driver.Navigate().GoToUrl(URL);

                // --- Login ---
                UIActions.Login(driver, wait, EMAIL, PASSWORD);
                UIActions.WaitForPageLoad(driver);

                // --- Team selection (if needed) ---
                if (UIActions.IsTeamSelectionScreen(driver))
                    UIActions.HandleTeamSelection(driver);

                // --- Fetch categories from main page ---
                Console.WriteLine("\nFetching available categories...");
                var categories = UIActions.GetCategories(driver);
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

                // --- Click the category (on main page) ---
                Console.WriteLine($"Clicking category: {selectedCategory.Name}");
                UIActions.ClickCategory(driver, selectedCategory);
                Thread.Sleep(2000); // wait for iframe to start loading

                // --- Find and switch to the map iframe ---
                var mapIframe = UIActions.FindMapIframe(driver);
                if (mapIframe == null) { Console.WriteLine("❌ Map iframe not found."); return; }
                driver.SwitchTo().Frame(mapIframe);
                Console.WriteLine("✅ Switched to map iframe.");

                // Wait for canvas to exist and have size
                var canvasWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                canvasWait.Until(d =>
                {
                    var canvas = d.FindElement(By.TagName("canvas"));
                    return canvas != null && canvas.Size.Width > 0 && canvas.Size.Height > 0;
                });
                Console.WriteLine("Canvas found and has size.");

                // Wait for canvas to update after category click
                var beforeColor = UIActions.GetCanvasCenterColor(driver);
                UIActions.WaitForCanvasUpdate(driver, beforeColor);
                Console.WriteLine("Canvas updated.");

                // Get the actual colour of the seats/GA area
                var actualColor = UIActions.GetActualCategoryColor(driver);
                if (actualColor == null) { Console.WriteLine("Could not determine seat color."); return; }
                Console.WriteLine($"Actual color: ({actualColor[0]}, {actualColor[1]}, {actualColor[2]})");

                // Extract seats using that colour
                var layout = CanvasAnalyzer.ExtractSeats(driver, actualColor[0], actualColor[1], actualColor[2],
                                                         DEFAULT_TOLERANCE, CLUSTER_DISTANCE);

                // If no seats found -> GA zone
                if (layout == null || layout.AllSeats.Count == 0)
                {
                    Console.WriteLine("No individual seats found – assuming GA zone.");
                    // Click on a GA area to open the popup
                    UIActions.ClickCanvasAtColor(driver, actualColor, DEFAULT_TOLERANCE);
                    Thread.Sleep(1000); // allow popup to appear

                    // Switch back to main page to handle the popup
                    driver.SwitchTo().DefaultContent();
                    if (UIActions.IsGAPopupPresent(driver))
                    {
                        UIActions.HandleGAPopup(driver, seatCount);
                        UIActions.ClickCheckout(driver);
                        Console.WriteLine("GA seats selected. Checkout initiated.");
                        while (true) Thread.Sleep(10000);
                    }
                    else
                    {
                        Console.WriteLine("GA popup did not appear.");
                        return;
                    }
                }

                // --- Individual seats selection ---
                Console.WriteLine($"Found {layout.AllSeats.Count} seats.");

                List<Seat> selectedSeats = null;
                if (requireConsecutive)
                {
                    selectedSeats = SeatSelector.FindConsecutiveSeats(layout, seatCount);
                    if (selectedSeats == null)
                    {
                        Console.WriteLine($"⚠️ No block of {seatCount} consecutive seats found.");
                        Console.Write("Continue with non-consecutive selection? (y/n): ");
                        if (Console.ReadLine().ToLower() != "y") return;
                        requireConsecutive = false;
                    }
                }
                if (!requireConsecutive && selectedSeats == null)
                    selectedSeats = SeatSelector.TakeFirstNSeats(layout.AllSeats, seatCount);

                if (selectedSeats == null || selectedSeats.Count < seatCount)
                {
                    Console.WriteLine($"❌ Not enough seats. Found {layout.AllSeats.Count}, requested {seatCount}.");
                    return;
                }

                Console.WriteLine($"🎯 Selecting {selectedSeats.Count} seats...");
                foreach (var seat in selectedSeats)
                    UIActions.ClickSeat(driver, seat);

                UIActions.ClickCheckout(driver);
                Console.WriteLine("🎉 SUCCESS: Seats selected and checkout initiated!");

                Console.WriteLine("\n✅ Task Finished. Browser is now in manual mode.");
                Console.WriteLine("Please complete the payment in the opened window.");
                while (true) { Thread.Sleep(10000); }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Runtime Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
            // No driver.Quit() – keep browser open
        }
    }
}