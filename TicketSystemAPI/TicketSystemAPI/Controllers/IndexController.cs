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

        public IndexController(IOptions<ElasticOptions> elasticOptions, IOptions<DbOptions> dbOptions)
        {
            _client = new ElasticClient(new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)));
            _dbOptions = dbOptions;
        }

        [HttpPost]
        [Route("Customers")]
        public async void IndexCustomers()
        {
            await _client.Indices.DeleteAsync("customers");

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var customers = await conn.QueryAsync<Customers>("SELECT * FROM Customers;");
                    await _client.BulkAsync(request => request
                    .Index("customers")
                    .IndexMany<Customers>(customers));
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        [HttpPost]
        [Route("Concerts")]
        public async void IndexConcerts()
        {
            await _client.Indices.DeleteAsync("concerts");

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var concerts = await conn.QueryAsync<Concerts>("SELECT * FROM Concerts;");
                    await _client.BulkAsync(request => request
                    .Index("concerts")
                    .IndexMany<Concerts>(concerts));
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        [HttpPost]
        [Route("Venues")]
        public async void IndexVenues()
        {
            await _client.Indices.DeleteAsync("venues");

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var venues = await conn.QueryAsync<Venues>("SELECT * FROM Venues;");
                    await _client.BulkAsync(request => request
                    .Index("venues")
                    .IndexMany<Venues>(venues));
                }
                catch(SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        [HttpPost]
        [Route("Artists")]
        public async void IndexArtists()
        {
            await _client.Indices.DeleteAsync("artists");

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var artists = await conn.QueryAsync<Artists>("SELECT * FROM Artists;");
                    await _client.BulkAsync(request => request
                    .Index("artists")
                    .IndexMany<Artists>(artists));
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }

        /*
        [HttpPost]
        [Route("Index")]
        public async void Index()
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    string sql = @"SELECT c.CalendarDate, c.Cancelled, c.Price, c.TicketsLeft, a.ArtistName, v.VenueName, v.Capacity, v.Coordinates
                                FROM Concerts c
                                INNER JOIN Artists a ON c.ArtistId = c.ArtistId
                                INNER JOIN Venues v ON c.VenueId = v.VenueId";
                    conn.Open();
                    IEnumerable<dynamic>indexPOCOs = await conn.QueryAsync(sql);

                    BulkDescriptor descriptor = new BulkDescriptor();

                    foreach (var doc in indexPOCOs)
                    {
                        descriptor.Index<object>(i => i
                            .Index("mainIndex")
                            .Document(doc));
                    }

                    await _client.BulkAsync(descriptor);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }*/
    }
}