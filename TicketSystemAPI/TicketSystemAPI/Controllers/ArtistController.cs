using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using TicketSystemAPI.Models;
using TicketSystemAPI.Utils;
using Dapper;
using Nest;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;
        private readonly ElasticClient _client;

        public ArtistController(IOptions<DbOptions> dbOptions, IOptions<ElasticOptions> elasticOptions)
        {
            _dbOptions = dbOptions;
            _client = new ElasticClient(new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)).DefaultMappingFor<Artists>(m => m.IndexName("artists")));
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
                    string insertSql = "INSERT INTO Artists (ArtistName) VALUES(@ArtistName);";
                    conn.Execute(insertSql, artist);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }

                string selectSql = "SELECT * FROM Artists WHERE ArtistName = @ArtistName;";
                Artists indexArtist = conn.Query<Artists>(selectSql, artist).FirstOrDefault();

                if (indexArtist != null)
                {
                    _client.IndexDocument(indexArtist);
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

            var deleteResponse = _client.DeleteByQuery<Artists>(s => s
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.ArtistId).Query(id.ToString()))));
        }
    }
}