using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using WeBook.Models;
using WeBook.Utilities;

namespace WeBook.Engines
{
    public static class MapInteractionEngine
    {
        public static bool SelectSectionOnMap(IWebDriver driver, SeatSelection selection)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                string script = $@"
            var sectionLabel = '{selection.Section}';
            if (window.chart && typeof window.chart.selectSection === 'function') {{
                window.chart.selectSection(sectionLabel);
                return true;
            }}
            var sections = document.querySelectorAll('[data-section-label]');
            for (var s of sections) {{
                if (s.getAttribute('data-section-label').toLowerCase() === sectionLabel.toLowerCase()) {{
                    s.click();
                    return true;
                }}
            }}
            return false;";

                // FIX: Safely handle the object return to avoid null unboxing
                var result = js.ExecuteScript(script);
                return result is bool b && b;
            }
            catch { return false; }
        }

        private static string ExtractRgb(string style)
        {
            // Simple helper to pull 'rgb(x, y, z)' out of a style string
            if (style.Contains("rgb"))
            {
                int start = style.IndexOf("rgb");
                int end = style.IndexOf(")", start) + 1;
                return style.Substring(start, end - start);
            }
            return style;
        }
    }
}