using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook.Models
{
    public class SeatRequest
    {
        public string TargetUrl { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
