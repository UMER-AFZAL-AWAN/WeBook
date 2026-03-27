using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace WeBook
{
    internal class Program
    {
        // ---------------- CONFIG ----------------
        static string EMAIL = "cabnipcar@bangban.uk";
        static string PASSWORD = "Aa@123456789";
        static string URL = "https://webook.com/en/events/rsl-al-khaleej-vs-al-hilal-387468/book";

        // Section Color (initial guess)
        static int R = 139, G = 195, B = 74;

        static void Main(string[] args)
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");

            Logger.Log("Starting browser...");

            var driver = new ChromeDriver(options);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            try
            {
                Logger.Log($"Opening URL: {URL}");
                driver.Navigate().GoToUrl(URL);

                HandleCookies(driver);

                // ---------------- LOGIN ----------------
                try
                {
                    Logger.Log("Attempting login...");

                    var email = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='auth_login_email_input']")));
                    email.SendKeys(EMAIL);

                    driver.FindElement(By.CssSelector("[data-testid='auth_login_password_input']"))
                          .SendKeys(PASSWORD);

                    driver.FindElement(By.CssSelector("[data-testid='auth_login_submit_button']")).Click();

                    Logger.Log("Login submitted (handle CAPTCHA manually)");
                }
                catch (Exception ex)
                {
                    Logger.Log("Login skipped");
                    Logger.LogError(ex);
                }

                // ---------------- WAIT FOR MAP ----------------
                Logger.Log("Waiting for stadium map...");
                bool mapReady = false;

                for (int attempt = 0; attempt < 25; attempt++)
                {
                    Logger.Log($"Map attempt: {attempt}");

                    driver.SwitchTo().DefaultContent();
                    var iframes = driver.FindElements(By.TagName("iframe"));
                    Logger.Log($"Found {iframes.Count} iframes");

                    foreach (var frame in iframes)
                    {
                        try
                        {
                            driver.SwitchTo().Frame(frame);

                            if (ScanCanvas(driver, R, G, B, false, false))
                            {
                                Logger.Log("Canvas detected");
                                mapReady = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex);
                        }

                        driver.SwitchTo().DefaultContent();
                    }

                    if (mapReady) break;
                    Thread.Sleep(1000);
                }

                if (!mapReady)
                {
                    Logger.Log("Map not found. Exiting.");
                    return;
                }

                // ---------------- CLICK SECTION ----------------
                Logger.Log("Clicking section...");
                ScanCanvas(driver, R, G, B, true, false);
                Thread.Sleep(4000);

                // ---------------- RE-DETECT IFRAME ----------------
                Logger.Log("Re-detecting iframe after section click...");
                driver.SwitchTo().DefaultContent();

                var framesAfter = driver.FindElements(By.TagName("iframe"));
                foreach (var frame in framesAfter)
                {
                    try
                    {
                        driver.SwitchTo().Frame(frame);

                        if (driver.FindElements(By.TagName("canvas")).Count > 0)
                        {
                            Logger.Log("New canvas found");
                            break;
                        }

                        driver.SwitchTo().DefaultContent();
                    }
                    catch { }
                }

                // ---------------- SELECT SEATS ----------------
                Logger.Log("Selecting seats...");
                int seats = 0;

                for (int i = 0; i < 15; i++)
                {
                    bool result = ScanCanvas(driver, R, G, B, true, true);
                    Logger.Log($"Seat attempt {i}: {result}");

                    if (result)
                    {
                        seats++;
                        Logger.Log($"Seats selected: {seats}");
                        Thread.Sleep(700);
                    }

                    if (seats >= 4) break;
                }

                // ---------------- CHECKOUT ----------------
                driver.SwitchTo().DefaultContent();
                Logger.Log("Searching checkout button...");

                var checkout = wait.Until(d =>
                    d.FindElements(By.XPath("//button[contains(., 'Checkout') or contains(., 'Next')]"))
                     .FirstOrDefault(e => e.Displayed && e.Enabled)
                );

                if (checkout != null)
                {
                    Logger.Log("Checkout found");
                    ((IJavaScriptExecutor)driver).ExecuteScript(
                        "arguments[0].scrollIntoView(true); arguments[0].click();", checkout);
                }
                else
                {
                    Logger.Log("Checkout NOT found");
                }

                // ---------------- TERMS ----------------
                Logger.Log("Clicking terms...");
                var terms = wait.Until(d => d.FindElement(By.CssSelector("input[type='checkbox']")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", terms);

                Logger.Log("DONE - proceed manually");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                Logger.Log("Waiting before exit...");
                Thread.Sleep(60000);
                driver.Quit();
            }
        }

        // ---------------- CANVAS SCAN ----------------
        static bool ScanCanvas(IWebDriver driver, int r, int g, int b, bool click, bool spread)
        {
            Logger.Log($"ScanCanvas | click={click} spread={spread}");

            string script = @"
                var canvas = document.querySelector('canvas');
                if (!canvas) return { found:false };

                var ctx = canvas.getContext('2d', { willReadFrequently: true });
                var data = ctx.getImageData(0, 0, canvas.width, canvas.height).data;

                var hits = [];

                for (var i = 0; i < data.length; i += 4) {

                    var rr = data[i];
                    var gg = data[i+1];
                    var bb = data[i+2];

                    if (Math.abs(rr-arguments[0])<20 &&
                        Math.abs(gg-arguments[1])<20 &&
                        Math.abs(bb-arguments[2])<20) {

                        var x = (i/4) % canvas.width;
                        var y = Math.floor((i/4) / canvas.width);

                        // IGNORE TOP AREA
                        if (y < canvas.height * 0.2) continue;

                        hits.push({x:x, y:y, r:rr, g:gg, b:bb});

                        if (hits.length >= 30) break;
                    }
                }

                if (hits.length === 0) return { found:false };

                var pick = hits[Math.floor(Math.random() * hits.length)];

                if (!arguments[3]) return pick;

                var rect = canvas.getBoundingClientRect();

                function fire(cx, cy) {
                    var ev = new MouseEvent('click', {
                        view: window,
                        bubbles: true,
                        clientX: rect.left + cx,
                        clientY: rect.top + cy
                    });
                    canvas.dispatchEvent(ev);
                }

                fire(pick.x, pick.y);

                if (arguments[4]) {
                    fire(pick.x+5, pick.y);
                    fire(pick.x-5, pick.y);
                    fire(pick.x, pick.y+5);
                }

                return pick;
            ";

            var result = (Dictionary<string, object>)
                ((IJavaScriptExecutor)driver).ExecuteScript(script, r, g, b, click, spread);

            if (!(bool)result["found"])
            {
                Logger.Log("Canvas NOT found");
                return false;
            }

            Logger.Log($"HIT -> X:{result["x"]} Y:{result["y"]} RGB:{result["r"]},{result["g"]},{result["b"]}");

            Thread.Sleep(200);
            return true;
        }

        // ---------------- COOKIES ----------------
        static void HandleCookies(IWebDriver driver)
        {
            try
            {
                Logger.Log("Checking cookies...");

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                var btn = wait.Until(d =>
                    d.FindElements(By.XPath("//button[contains(., 'Accept')]"))
                     .FirstOrDefault(e => e.Displayed)
                );

                if (btn != null)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
                    Logger.Log("Cookies accepted");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("No cookies popup");
                Logger.LogError(ex);
            }
        }
    }

    // ---------------- LOGGER ----------------
    //static class Logger
    //{
    //    private static readonly string FilePath = "webook_log.txt";

    //    public static void Log(string msg)
    //    {
    //        string line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
    //        Console.WriteLine(line);
    //        File.AppendAllText(FilePath, line + Environment.NewLine);
    //    }

    //    public static void LogError(Exception ex)
    //    {
    //        Log("ERROR: " + ex.Message);
    //        Log("STACK: " + ex.StackTrace);
    //    }
    //}
}
