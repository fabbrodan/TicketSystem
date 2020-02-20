using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace TicketSystemAPI.Models
{
    [ElasticsearchType(IdProperty = nameof(ArtistId))]
    public class Artists
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
    }
}
