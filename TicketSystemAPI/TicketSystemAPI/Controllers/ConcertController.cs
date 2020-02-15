﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using Dapper;
using TicketSystemAPI.Models;
using TicketSystemAPI.Utils;
using Nest;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConcertController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;
        private readonly ElasticClient _client;

        public ConcertController(IOptions<DbOptions> dbOptions, IOptions<ElasticOptions> elasticOptions)
        {
            _dbOptions = dbOptions;
            _client = new ElasticClient(new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)).DefaultMappingFor<Concerts>(m => m.IndexName("concerts")));
        }

        [HttpGet]
        public IEnumerable<Concerts> Get()
        {
            IEnumerable<Concerts> concerts = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Concerts;";
                    concerts = conn.Query<Concerts>(sql);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return concerts;
        }

        [HttpGet("{id}")]
        public Concerts Get(int id)
        {
            Concerts concert = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Concerts WHERE ConcertId = @concertId;";
                    concert = conn.Query(sql, new { concertId = id }).FirstOrDefault();
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return concert;
        }

        [HttpPost]
        public void Post([FromBody] Concerts concert)
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string insertSql = "INSERT INTO Concerts(ArtistId, VenueId, CalendarDate, Price)" +
                        "VALUES(@ArtistId, @VenueId, @CalendarDate, @Price);";
                    conn.Execute(insertSql, concert);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }

                string selectSql = "SELECT * FROM Concerts WHERE ArtistId = @ArtistId AND VenueId = @VenueId AND CalendarDate = @CalendarDate;";
                Concerts indexConcert = conn.Query<Concerts>(selectSql, concert).FirstOrDefault();

                if (indexConcert != null)
                {
                    _client.IndexDocument(indexConcert);
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
                    string sql = "DELETE FROM Concerts WHERE ConcertId = @concertId;";
                    conn.Execute(sql, new { concertId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            var deleteResponse = _client.DeleteByQuery<Concerts>(r => r
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.ConcertId)
                            .Query(id.ToString()))));
        }

        [HttpPost]
        [Route("purchase/{id}")]
        public string Purchase(int id, [FromBody] Customers customer)
        {
            Concerts concert = null;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string concertSelect = "SELECT * FROM Concerts WHERE ConcertId = @concertId;";
                    concert = conn.Query<Concerts>(concertSelect, new { concertId = id }).FirstOrDefault<Concerts>();

                    string customerSelect = "SELECT * FROM Customers WHERE CustomerId = @customerId;";
                    customer = conn.Query<Customers>(customerSelect, new { customerId = customer.CustomerId }).FirstOrDefault<Customers>();

                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            if (concert.Price > customer.Currency)
            {
                return "Not enough money";
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        conn.Execute("SP_Purchase", new { concertId = concert.ConcertId, customerId = customer.CustomerId }, commandType: CommandType.StoredProcedure);
                    }
                    catch (SqlException exc)
                    {
                        Console.WriteLine(exc.Message);
                    }

                    string selectSql = "SELECT * FROM Concerts WHERE ArtistId = @ArtistId AND VenueId = @VenueId AND CalendarDate = @CalendarDate;";
                    Concerts indexConcert = conn.Query(selectSql, concert).FirstOrDefault();

                    if (indexConcert != null)
                    {
                        _client.IndexDocument(indexConcert);
                    }
                }

                return "Bought!";
            }
        }
    }
}