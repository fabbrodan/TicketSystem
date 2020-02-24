using System;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;
using Dapper;
using Nest;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

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
            var settings = new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)).DefaultMappingFor<IndexObject>(m => m.IndexName("mainindex"));
            _client = new ElasticClient(settings);
            _dbOptions = dbOptions;
        }

        [HttpPost]
        [Route("Get")]
        public IEnumerable<IndexObject> Get([FromQuery] string searchParam)
        {
            var dateSearchTerm = DateTime.MinValue;
            DateTime.TryParse(searchParam, out dateSearchTerm);        

            if (searchParam == null || String.IsNullOrEmpty(searchParam))
            {
                return _client.Search<IndexObject>(s => s.Query(q => q.MatchAll())).Documents;
            }

            searchParam = searchParam.Insert(0, "*").Insert(searchParam.Length + 1, "*").ToLower().Trim();
            var searchResponse = _client.Search<IndexObject>(s => s
            .Query(q => q
            .QueryString(qs => qs
            .Fields(f => f.Field(a => a.ArtistName, 3.0)
                .Field(v => v.VenueName, 2.0)
                .Field(c => c.City, 1.5)
                .Field(d => d.ConcertDate.ToShortDateString()))
            //.AllowLeadingWildcard()
            .Fuzziness(Fuzziness.EditDistance(3))
            .Query(searchParam))).Explain(true));

            return searchResponse.Documents;
            
        }
    }
}