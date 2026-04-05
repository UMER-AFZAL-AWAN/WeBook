using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WeBook.Services
{
    public static class TeamSelectionScreenService
    {
        #region IsTeamSelectionScreen
        // --- Team Selection ---
        public static bool IsTeamSelectionScreen(IWebDriver driver)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var buttons = wait.Until(d => d.FindElements(By.CssSelector("button[data-testid^='ui_toggle_favorite_team_']")));
                return buttons.Count > 0;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public static void HandleTeamSelection(IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            var teamButtons = driver.FindElements(By.CssSelector("button[data-testid^='ui_toggle_favorite_team_']"));
            if (teamButtons.Count == 0)
            {
                Console.WriteLine("No team selection buttons found. Skipping team selection.");
                return;
            }

            var teams = new List<string>();
            foreach (var btn in teamButtons)
            {
                var img = btn.FindElement(By.TagName("img"));
                string teamName = img.GetAttribute("alt");
                teams.Add(teamName);
            }

            Console.WriteLine("\nChoose your favorite team:");
            for (int i = 0; i < teams.Count; i++)
                Console.WriteLine($"{i + 1}. {teams[i]}");
            Console.Write("Enter team number: ");
            int choice = int.Parse(Console.ReadLine()) - 1;
            if (choice < 0 || choice >= teams.Count)
            {
                Console.WriteLine("Invalid choice. Exiting.");
                return;
            }

            teamButtons[choice].Click();
            Thread.Sleep(500);

            var checkbox = driver.FindElement(By.CssSelector("button[data-testid='ticketing_teams_terms_checkbox']"));
            if (checkbox.GetAttribute("data-state") != "checked")
            {
                checkbox.Click();
                Thread.Sleep(500);
            }

            var nextButton = wait.Until(drv => drv.FindElement(By.XPath("//button[contains(., 'Next: Select Tickets')]")));
            wait.Until(ExpectedConditions.ElementToBeClickable(nextButton));
            nextButton.Click();

            wait.Until(drv => drv.FindElements(By.CssSelector("button[data-open]")).Count > 0 ||
                             drv.FindElements(By.Id("seats-cloud-chart")).Count > 0);
            Thread.Sleep(2000);
        }
        #endregion
    }
}
