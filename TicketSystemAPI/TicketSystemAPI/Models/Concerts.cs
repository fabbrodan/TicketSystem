using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSystemAPI.Models
{
    public class Concerts
    {
        public int ConcertId { get; set; }
        public int ArtistId { get; set; }
        public int LocationId { get; set; }
        public DateTime CalendarDate { get; set; }
    }
}
