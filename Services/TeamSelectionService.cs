using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WeBook.Utilities;

namespace WeBook.Services
{
    public static class TeamSelectionService
    {
        public static void SelectTeamIfPresent(IWebDriver driver)
        {
            try
            {
                // Look for team selection buttons (usually contains 'team-card' or 'select-team')
                var teamButtons = driver.FindElements(By.CssSelector("div[class*='team'], button[id*='team']"));
                if (teamButtons.Any())
                {
                    Logger.Log("Team selection page detected. Selecting first available team...");
                    teamButtons.First().Click();
                    Thread.Sleep(2000); // Wait for transition
                }
            }
            catch { /* Not on a team selection page, skip */ }
        }
    }
}