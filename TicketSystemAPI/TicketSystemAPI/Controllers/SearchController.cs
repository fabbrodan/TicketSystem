using System;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;
using Dapper;
using Nest;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ElasticClient _client;
        private readonly IOptions<DbOptions> _dbOptions;

        public SearchController(IOptions<DbOptions> dbOptions, IOptions<ElasticOptions> elasticOptions)
        {
            _client = new ElasticClient(new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)));
            _dbOptions = dbOptions;
        }

        [HttpPost]
        [Route("Get")]
        public IEnumerable<IndexObject> Get([FromQuery] string searchParam)
        {
            // FIX THIS BULL SHIT CURRENTLY WORKS LIKE A CARTESIAN PRODUCT SEARCH?!?!?!
            var searchResponse = _client
                .Search<IndexObject>(s => s
                .Index("mainindex")
                    .Query(q => q
                        .MultiMatch(mu => mu
                            .Fuzziness(Fuzziness.Auto)
                            .FuzzyRewrite(MultiTermQueryRewrite.ConstantScore)
                            .FuzzyTranspositions(true)
                            .Fields(f => f
                                .Field(v => v.VenueName, 2.2)
                                .Field(a => a.ArtistName, 1.5)))));

            IEnumerable<IndexObject> hits = searchResponse.Documents;

            return hits;
            
        }
    }
}