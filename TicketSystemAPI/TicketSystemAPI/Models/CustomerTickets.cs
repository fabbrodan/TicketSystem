using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSystemAPI.Models
{
    public class CustomerTickets
    {
        public int CustomerId { get; set; }
        public int TicketId { get; set; }
        public DateTime SoldDate { get; set; }
    }
}
