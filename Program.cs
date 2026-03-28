using OpenQA.Selenium;
using System;
using System.Threading;
using WeBook.Config;
using WeBook.Helpers;
using WeBook.Services;

namespace WeBook
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.Separator();
            Logger.Info("WeBook Seat Selection Bot Starting");
            Logger.Separator();

            // Verify URL before starting
            if (!IsValidUrl(AppConfig.Url))
            {
                Logger.Error("Invalid URL in configuration. Please check AppConfig.cs");
                Logger.Info($"Current URL: {AppConfig.Url}");
                Logger.Info("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            using (var webDriver = new WebDriverService())
            {
                var driver = webDriver.Driver;

                // Initialize services
                var navigation = new NavigationService(driver);
                var login = new LoginService(driver);
                var frameManager = new FrameManager(driver);
                var seatSelector = new SeatSelectionService(driver);

                try
                {
                    // Step 1: Navigate to URL
                    Logger.Step("Step 1: Navigate to Event Page");
                    bool navigated = webDriver.NavigateToUrl(AppConfig.Url);

                    if (!navigated)
                    {
                        Logger.Error("Failed to navigate to URL");
                        return;
                    }

                    // Wait for initial page load
                    Logger.Info("Waiting for page to load...");
                    Thread.Sleep(AppConfig.MediumDelay * 4);

                    // Step 2: Handle Cookies
                    Logger.Step("Step 2: Handle Cookies");
                    navigation.HandleCookies();

                    // Step 3: Login
                    Logger.Step("Step 3: Login");
                    login.Login(AppConfig.Email, AppConfig.Password);

                    // Wait after login for page to stabilize
                    Logger.Info("Waiting for page after login...");
                    Thread.Sleep(AppConfig.ExtraLongDelay);

                    // Step 4: Find Map Frame (with proper waiting)
                    Logger.Step("Step 4: Find Stadium Map");

                    // Wait for the page to be fully rendered after login
                    Logger.Info("Waiting for stadium map to load...");
                    Thread.Sleep(AppConfig.LongDelay * 2);

                    if (!frameManager.FindMapFrame())
                    {
                        Logger.Error("Cannot proceed without map");
                        return;
                    }

                    // Extra wait after finding map
                    Thread.Sleep(AppConfig.MediumDelay * 2);

                    // Step 5: Click Available Section
                    Logger.Step("Step 5: Click Available Section");
                    bool sectionClicked = false;
                    for (int attempt = 0; attempt < 10; attempt++)
                    {
                        frameManager.EnterMapFrame();

                        // Wait for canvas to be interactive
                        Thread.Sleep(1000);

                        sectionClicked = seatSelector.ClickSection(
                            AppConfig.AvailableColor[0],
                            AppConfig.AvailableColor[1],
                            AppConfig.AvailableColor[2]
                        );

                        if (sectionClicked)
                        {
                            Logger.Success("Section clicked successfully!");
                            break;
                        }

                        Logger.Warning($"Section click attempt {attempt + 1} failed, retrying...");
                        Thread.Sleep(AppConfig.LongDelay);
                    }

                    if (!sectionClicked)
                    {
                        Logger.Error("Could not click any section");
                        return;
                    }

                    // Step 6: Wait for Seats to Load
                    Logger.Step("Step 6: Wait for Seat Grid");
                    Logger.Info("Waiting for seat selection to load...");

                    // Wait for the seat grid to appear after section click
                    Thread.Sleep(AppConfig.ExtraLongDelay);

                    // Additional wait for canvas to redraw with seats
                    frameManager.EnterMapFrame();
                    Thread.Sleep(AppConfig.LongDelay);

                    // Step 7: Select Seats
                    Logger.Step("Step 7: Select Seats");
                    seatSelector.ResetClickedPositions();

                    int seatsSelected = 0;
                    int failedAttempts = 0;
                    int maxAttempts = 25;

                    while (seatsSelected < AppConfig.MaxSeats && failedAttempts < maxAttempts)
                    {
                        frameManager.EnterMapFrame();

                        // Wait a bit before each seat click
                        Thread.Sleep(800);

                        bool seatClicked = seatSelector.ClickSeat(
                            AppConfig.AvailableColor[0],
                            AppConfig.AvailableColor[1],
                            AppConfig.AvailableColor[2]
                        );

                        if (seatClicked)
                        {
                            Logger.Debug("Waiting for popup...");
                            Thread.Sleep(AppConfig.MediumDelay);

                            frameManager.ExitToMainContent();

                            // Wait for popup to appear
                            Logger.Debug("Checking for quantity popup...");
                            Thread.Sleep(500);

                            bool popupHandled = seatSelector.HandleQuantityPopup(AppConfig.MaxSeats - seatsSelected);

                            if (popupHandled)
                            {
                                seatsSelected++;
                                failedAttempts = 0;
                                Logger.Success($"Seat {seatsSelected}/{AppConfig.MaxSeats} selected");
                                Thread.Sleep(AppConfig.MediumDelay * 2);
                            }
                            else
                            {
                                Logger.Warning("Popup not handled, retrying...");
                                failedAttempts++;
                            }
                        }
                        else
                        {
                            failedAttempts++;
                            Logger.Debug($"No seat found (attempt {failedAttempts}/{maxAttempts})");

                            // Scroll occasionally
                            if (failedAttempts % 3 == 0)
                            {
                                frameManager.EnterMapFrame();
                                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 250);");
                                Logger.Debug("Scrolled to find more seats");
                                Thread.Sleep(AppConfig.MediumDelay);
                            }
                        }

                        Thread.Sleep(AppConfig.ShortDelay);
                    }

                    // Step 8: Check Results
                    Logger.Step("Step 8: Check Results");
                    if (seatsSelected == 0)
                    {
                        Logger.Error("No seats were selected!");
                        return;
                    }

                    Logger.Success($"Successfully selected {seatsSelected} out of {AppConfig.MaxSeats} seats!");

                    // Step 9: Checkout
                    Logger.Step("Step 9: Proceed to Checkout");
                    frameManager.ExitToMainContent();

                    // Wait for checkout button to be ready
                    Thread.Sleep(AppConfig.LongDelay);
                    seatSelector.ProceedToCheckout();

                    Logger.Separator();
                    Logger.Success("✨ Process completed!");
                    Logger.Info("Browser will close in 60 seconds...");
                    Logger.Separator();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unexpected error: {ex.Message}");
                    Logger.Debug($"Stack trace: {ex.StackTrace}");
                }
                finally
                {
                    Thread.Sleep(60000);
                }
            }
        }

        static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return false;

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            return result && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}