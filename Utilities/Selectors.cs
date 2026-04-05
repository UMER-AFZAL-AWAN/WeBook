using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook.Utilities
{
    public static class Selectors
    {
        public const string LoginEmailInput = "input[type='email']";
        public const string StadiumMapFrame = "iframe[id*='seat-cloud']";
        public const string CookieRejectBtn = "//button[contains(., 'Reject')]";
    }
}
