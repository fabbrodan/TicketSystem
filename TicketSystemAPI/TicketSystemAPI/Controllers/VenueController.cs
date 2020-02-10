using System;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VenueController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;

        public VenueController(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }
        
        [HttpGet]
        public IEnumerable<Venues> Get()
        {
            IEnumerable<Venues> venues = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Venues;";
                    venues = conn.Query<Venues>(sql);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return venues;
        }

        
        [HttpGet("{id}")]
        public Venues Get(int id)
        {
            Venues venue = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Venues WHERE VenueId = @venueId;";
                    venue = conn.Query<Venues>(sql, new { venueId = id }).FirstOrDefault<Venues>();
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
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
                    string sql = "INSERT INTO Venues (VenueName, Coordinates)" +
                        "VALUES(@VenueName, @Coordinates);";
                    conn.Execute(sql, venue);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
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
        }
    }
}
