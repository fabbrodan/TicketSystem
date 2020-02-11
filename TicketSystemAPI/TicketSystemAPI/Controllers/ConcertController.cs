﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using Dapper;
using TicketSystemAPI.Models;
using TicketSystemAPI.Utils;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConcertController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;

        public ConcertController(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions;
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
                    string sql = "INSERT INTO Concerts(ArtistId, VenueId, CalendarDate, Price)" +
                        "VALUES(@ArtistId, @VenueId, @CalendarDate, @Price);";
                    conn.Execute(sql, concert);
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
                    string sql = "DELETE FROM Concerts WHERE ConcertId = @concertId;";
                    conn.Execute(sql, new { concertId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
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
                }

                return "Bought!";
            }
        }
    }
}