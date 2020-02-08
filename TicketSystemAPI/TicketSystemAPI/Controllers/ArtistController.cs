using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using TicketSystemAPI.Models;
using TicketSystemAPI.Utils;
using Dapper;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;

        public ArtistController(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        [HttpGet]
        public IEnumerable<Artists> Get()
        {
            IEnumerable<Artists> artists = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Artists;";
                    artists = conn.Query<Artists>(sql);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return artists;
        }

        [HttpGet("{id}")]
        public Artists Get(int id)
        {
            Artists artist = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Artists WHERE ArtistId = @artistId;";
                    artist = conn.Query<Artists>(sql, new { artistId = id }).FirstOrDefault<Artists>();
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return artist;
        }

        [HttpPost]
        public void Post([FromBody] Artists artist)
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "INSERT INTO Artists (ArtistName) VALUES(@ArtistName);";
                    conn.Execute(sql, artist);
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
                    string sql = "DELETE FROM Artists WHERE ArtistId = @artistId;";
                    conn.Execute(sql, new { artistId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
        }
    }
}