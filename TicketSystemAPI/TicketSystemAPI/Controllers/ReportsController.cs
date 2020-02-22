using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Utils;
using Microsoft.Extensions.Options;
using Dapper;
using Microsoft.Data.SqlClient;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;

        public ReportsController(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        [HttpGet]
        [Route("PeriodSales")]
        public dynamic PeriodSales([FromQuery] string startDate, [FromQuery] string endDate)
        {
            dynamic Result = null;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT SUM(1) as theCount, SUM(c.Price) as Revenue
                                    FROM Concerts c
                                    INNER JOIN Tickets t ON c.ConcertId = t.ConcertId
                                    INNER JOIN CustomerTickets ct ON t.TicketId = ct.TicketId
                                    WHERE ct.SoldDate BETWEEN @StartDate AND @EndDate;";

                    Result = conn.Query<dynamic>(sql, new { StartDate = startDate, EndDate = endDate }).FirstOrDefault();
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return Result;
        }

        [HttpGet]
        [Route("TopTenArtists")]
        public dynamic TopTenArtists([FromQuery] string startDate, [FromQuery] string endDate)
        {
            IEnumerable<dynamic> Results = null;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT TOP 10 SUM(c.Price) as sumPrice, a.ArtistName, RANK() OVER (ORDER BY SUM(c.Price) DESC) as _rank
                                    FROM Concerts c
                                    INNER JOIN Artists a ON c.ArtistId = a.ArtistId
                                    INNER JOIN Tickets t ON c.ConcertId = t.ConcertId
                                    INNER JOIN CustomerTickets ct ON t.TicketId = ct.TicketId
                                    WHERE ct.SoldDate BETWEEN @StartDate AND @EndDate
                                    GROUP BY a.ArtistName
                                    ORDER BY _rank DESC;";

                    Results = conn.Query<dynamic>(sql, new { StartDate = startDate, EndDate = endDate });
                }
                catch(SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return Results;
        }

        [HttpGet]
        [Route("CouponReport")]
        public dynamic CouponReport()
        {
            IEnumerable<dynamic> Results = null;
            using(SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT cc.CouponId, t.TicketId, c.ConcertId, c.Price, cc.ExpirationDate
                                    FROM Coupons cc
                                    INNER JOIN Tickets t ON cc.TicketId = t.TicketId
                                    INNER JOIN Concerts c ON t.ConcertId = c.ConcertId;";

                    Results = conn.Query<dynamic>(sql);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return Results;
        }
    }
}