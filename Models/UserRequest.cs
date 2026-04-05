using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeBook.Models
{
    public class UserRequest
    {
        public string TargetUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string SelectedTeam { get; set; } = string.Empty;
    }
}
