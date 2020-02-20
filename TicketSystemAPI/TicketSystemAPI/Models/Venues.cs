using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace TicketSystemAPI.Models
{
    [ElasticsearchType(IdProperty = nameof(VenueId))]
    public class Venues
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public int Capacity { get; set; }
        public string Coordinates { get; set; }
        public string City { get; set; }
    }
}
