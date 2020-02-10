using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;
using Nest;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using Dapper;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        private readonly ElasticClient _client;
        private readonly IOptions<DbOptions> _dbOptions;

        public IndexController(IOptions<ElasticOptions> options, IOptions<DbOptions> dbOptions)
        {
            _client = new ElasticClient(new ConnectionSettings(new Uri(options.Value.ClusterUrl)));
            _dbOptions = dbOptions;
        }

        [HttpPost]
        [Route("Customers")]
        public void IndexCustomers()
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    _client.Bulk(request => request
                    .Index("customers")
                    .IndexMany<Customers>(conn.Query<Customers>("SELECT * from Customers;")));
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        [HttpPost]
        [Route("Concerts")]
        public void IndexConcerts()
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    _client.Bulk(request => request
                    .Index("concerts")
                    .IndexMany<Concerts>(conn.Query<Concerts>("SELECT * FROM Concerts;")));
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }
    }
}