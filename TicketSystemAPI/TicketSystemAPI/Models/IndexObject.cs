using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSystemAPI.Models
{
    public class IndexObject
    {
        public int ConcertId { get; set; }
        public string VenueName { get; set; }
        public string ArtistName { get; set; }
        public decimal ConcertPrice { get; set; }
        public DateTime ConcertDate { get; set; }
        public int VenueCapacity { get; set; }
        public bool Cancelled { get; set; }
        public int TicketsLeft { get; set; }
        public string Coordinates { get; set; }
    }
}
