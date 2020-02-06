using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSystemAPI.Models
{
    public class Administrators
    {
        public int AdminId { get; set; }
        public string LoginId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
