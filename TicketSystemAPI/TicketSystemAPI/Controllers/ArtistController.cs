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
            var searchResponse = _client.Search<Artists>(s => s.MatchAll());

            IEnumerable<Artists> artists = searchResponse.Documents;

            return artists;
        }

        [HttpGet("{id}")]
        public Artists Get(int id)
        {
            var response = _client.Search<Artists>(s => s
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.ArtistId)
                            .Query(id.ToString()))));

            return response.Documents.FirstOrDefault();
        }

        [HttpPost]
        public Artists Post([FromBody] Artists artist)
        {
            Artists indexArtist = null;

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
                indexArtist = conn.Query<Artists>(selectSql, artist).FirstOrDefault();

                if (indexArtist != null)
                {
                    _client.IndexDocument(indexArtist);
                }
            }

            return indexArtist;
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

            var deleteResponse = _client.DeleteByQuery<Artists>(r => r
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.ArtistId).Query(id.ToString()))));
        }
    }
}