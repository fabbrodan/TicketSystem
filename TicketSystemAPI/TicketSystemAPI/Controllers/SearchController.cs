using System;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;
using Dapper;
using Nest;
using Microsoft.Extensions.Options;

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
    }
}