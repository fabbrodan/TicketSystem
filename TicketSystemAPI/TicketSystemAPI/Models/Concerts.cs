﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace TicketSystemAPI.Models
{
    [ElasticsearchType(IdProperty = nameof(ConcertId))]
    public class Concerts
    {
        public int ConcertId { get; set; }
        public int ArtistId { get; set; }
        public int VenueId { get; set; }
        public DateTime CalendarDate { get; set; }

        public bool Cancelled { get; set; }
        public decimal Price { get; set; }
        public int TicketsLeft { get; set; }
    }
}
