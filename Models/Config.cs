using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook.Models
{
    public static class Config
    {
        // System-wide settings
        public const string BaseUrl = "https://webook.com/en";
        public const int DefaultTimeout = 45;
        public const bool HeadlessMode = false; // Set to true to run without a visible window
        public const string BrowserType = "Chrome";
    }
}
