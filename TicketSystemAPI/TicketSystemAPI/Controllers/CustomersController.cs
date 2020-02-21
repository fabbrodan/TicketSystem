using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Models;
using PasswordHasher;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Http;
using TicketSystemAPI.Utils;
using Microsoft.Extensions.Options;
using Nest;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ElasticClient _client;
        private readonly IOptions<DbOptions> _dbOptions;

        public CustomersController (IOptions<DbOptions> dbOptions, IOptions<ElasticOptions> elasticOptions)
        {
            _dbOptions = dbOptions;
            _client = new ElasticClient(new ConnectionSettings(new Uri(elasticOptions.Value.ClusterUrl)).DefaultMappingFor<Customers>(m => m.IndexName("customers")));
        }

        [HttpGet]
        public IEnumerable<Customers> Get()
        {
            IEnumerable<Customers> customers = null;

            var searchResponse = _client.Search<Customers>(s => s.MatchAll());

            customers = searchResponse.Documents;

            return customers;
        }

        [HttpGet("{id}")]
        public Customers Get(int id)
        {
            Customers customer = null;

            var searchResponse = _client.Search<Customers>(s => s
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.CustomerId)
                            .Query(id.ToString()))));

            customer = searchResponse.Documents.FirstOrDefault();

            return customer;
        }

        [HttpPost]
        [Route("AddFunds")]
        public Customers AddFunds([FromBody] Customers customer)
        {
            Customers returnCust = null;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE Customers SET Currency = Currency + @currency WHERE CustomerId = @customerId;";
                    conn.Execute(sql, new { currency = customer.Currency, customerId = customer.CustomerId });

                    string returnSql = "SELECT * FROM Customers WHERE CustomerId = @customerId;";
                    returnCust = conn.Query<Customers>(returnSql, new { customerId = customer.CustomerId }).FirstOrDefault<Customers>();
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }

                _client.IndexDocument(returnCust);
            }

            return returnCust;
        }

        [HttpPost]
        [Route("NewUser")]
        public Customers NewUser([FromBody] Customers customer)
        {
            Customers newCustomer = customer;
            Customers returnCustomer = null;

            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();

            newCustomer.PasswordSalt = hasher.RandomSalt;
            newCustomer.Password = hasher.GenerateSaltedHash(newCustomer.Password);

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "INSERT INTO Customers (LoginName, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate)" +
                        "VALUES(@loginName, @email, @phoneNumber, @password, @passwordSalt, @registeredDate)";

                    conn.Execute(sql,
                        new
                        {
                            loginName = newCustomer.LoginName,
                            email = newCustomer.Email,
                            phoneNumber = newCustomer.PhoneNumber,
                            password = newCustomer.Password,
                            passwordSalt = newCustomer.PasswordSalt,
                            registeredDate = DateTime.Now
                        });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }

                string getCustomer = "SELECT * FROM Customers WHERE LoginName = @LoginName AND Email = @Email;";
                returnCustomer = conn.Query<Customers>(getCustomer, new { newCustomer.LoginName, newCustomer.Email }).FirstOrDefault();
            }

            _client.IndexDocument<Customers>(returnCustomer);

            return returnCustomer;
        }

        [HttpPost]
        [Route("Login")]
        public Customers Login([FromBody] Customers customer)
        {

            Customers returnCust = null;
            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "SELECT Password, PasswordSalt FROM Customers WHERE Email = @Email AND IsActive = 1;";
                    var result = conn.Query(sql, new { customer.Email }).FirstOrDefault();
                    
                    if (result != null)
                    {
                        if (hasher.VerifyPassword(customer.Password, result.PasswordSalt, result.Password))
                        {
                            string getSql = "SELECT * FROM Customers WHERE Email = @Email AND IsActive = 1;";
                            returnCust = conn.Query<Customers>(getSql, new { customer.Email }).FirstOrDefault();
                        }
                    }

                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return returnCust;
        }

        [HttpDelete("{id}")]
        public HttpResponseMessage Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "DELETE FROM Customers WHERE CustomerId = @customerId;";

                    if (conn.Execute(sql, new { customerId = id }) != 1)
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

            }

            var deleteResonse = _client.DeleteByQuery<Customers>(r => r
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.CustomerId)
                            .Query(id.ToString()))));

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("{id}/Tickets")]
        public dynamic GetTickets(int id)
        {
            IEnumerable<dynamic> CustomerTickets = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT cust.LoginName, a.ArtistName, v.VenueName, c.CalendarDate, ct.SoldDate, c.Cancelled, ct.TicketId
                                    FROM Customers cust
                                    INNER JOIN CustomerTickets ct ON ct.CustomerId = cust.CustomerId
                                    INNER JOIN Tickets t ON ct.TicketId = t.TicketId
                                    INNER JOIN Concerts c ON t.ConcertId = c.ConcertId
                                    INNER JOIN Artists a ON c.ArtistId = a.ArtistId
                                    INNER JOIN Venues v ON c.VenueId = v.VenueId
                                    WHERE cust.CustomerId = @CustomerId;";

                    CustomerTickets = conn.Query(sql, new { CustomerId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return CustomerTickets;
        }

        [HttpGet]
        [Route("{id}/Coupons")]
        public dynamic GetCoupons(int id)
        {
            IEnumerable<dynamic> CustomerCoupons = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT c.LoginName, cc.CouponId, con.Price, cc.ExpirationDate, con.ConcertId
                                    FROM Concerts con
                                    INNER JOIN Tickets t ON con.ConcertId = t.ConcertId
                                    INNER JOIN Coupons cc ON t.TicketId = cc.TicketId
                                    INNER JOIN CustomerTickets ct ON t.TicketId = ct.TicketId
                                    INNER JOIN Customers c ON ct.CustomerId = c.CustomerId
                                    WHERE ct.CustomerId = @CustomerId
                                    AND con.Cancelled = 1";

                    CustomerCoupons = conn.Query<dynamic>(sql, new { CustomerId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return CustomerCoupons;
        }
    }
}
