using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook.Models
{
    public class Category
    {
        public string Name { get; set; }
        public int[] Rgb { get; set; } // [R, G, B]
        public int AvailableSeats { get; set; }
    }

}
