using System;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;
using Nest;
using System.Net;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenueController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;
        private readonly ElasticClient _client;

        public VenueController(IOptions<DbOptions> dbOptions, IOptions<ElasticOptions> elasticOptions)
        {
            _dbOptions = dbOptions;
            _client = new ElasticClient(new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)).DefaultMappingFor<Venues>(m => m.IndexName("venues")));
        }
        
        [HttpGet]
        public IEnumerable<Venues> Get()
        {
            IEnumerable<Venues> venues = null;
            var searchResponse = _client.Search<Venues>(s => s.MatchAll());
            venues = searchResponse.Documents;
            return venues;
        }

        
        [HttpGet("{id}")]
        public Venues Get(int id)
        {
            Venues venue = null;
            var searchResponse = _client.Search<Venues>(s => s
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.VenueId)
                            .Query(id.ToString()))));

            venue = searchResponse.Documents.FirstOrDefault();

            return venue;
        }

        
        [HttpPost]
        public void Post([FromBody] Venues venue)
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string insertSql = "INSERT INTO Venues (VenueName, Coordinates, Capacity, City)" +
                        "VALUES(@VenueName, @Coordinates, @Capacity, @City);";
                    conn.Execute(insertSql, venue);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }

                string selectSql = "SELECT * FROM Venues WHERE VenueName = @VenueName;";
                Venues indexVenue = conn.Query<Venues>(selectSql, venue).FirstOrDefault();
                
                if(indexVenue != null)
                {
                    _client.IndexDocument(indexVenue);
                }
            }
        }

        
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "DELETE FROM Venues WHERE VenueId = @venueId;";
                    conn.Execute(sql, new { venueId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            var deleteResponse = _client.DeleteByQuery<Venues>(r => r
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.VenueId).Query(id.ToString()))));
        }
    }
}
