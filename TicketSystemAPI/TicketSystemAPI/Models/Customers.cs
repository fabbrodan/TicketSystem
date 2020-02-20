using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace TicketSystemAPI.Models
{
    [ElasticsearchType(IdProperty = nameof(CustomerId))]
    public class Customers
    {
        public int CustomerId { get; set; }
        public string LoginName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string RegisteredDate { get; set; }
        public decimal Currency { get; set; }
        public bool IsActive { get; set; }
    }
}
