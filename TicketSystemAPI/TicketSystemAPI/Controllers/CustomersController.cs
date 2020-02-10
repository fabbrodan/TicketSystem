using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Models;
using PasswordHasher;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Http;
using TicketSystemAPI.Utils;
using Microsoft.Extensions.Options;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {

        private readonly IOptions<DbOptions> _dbOptions;

        public CustomersController (IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        [HttpGet]
        public IEnumerable<Customers> Get()
        {
            IEnumerable<Customers> customers = null;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Customers;";
                    customers = conn.Query<Customers>(sql);
                }
                catch(SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return customers;
        }

        [HttpGet("{id}")]
        public Customers Get(int id)
        {
            IEnumerable<Customers> customer = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Customers WHERE CustomerId = @customerId";
                    customer = conn.Query<Customers>(sql, new { customerId = id });
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return customer.FirstOrDefault();
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
            }

            return returnCust;
        }

        [HttpPost]
        [Route("NewUser")]
        public Customers NewUser([FromBody] Customers customer)
        {
            Customers newCustomer = customer;

            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();

            newCustomer.PasswordSalt = hasher.RandomSalt;
            newCustomer.Password = hasher.GenerateSaltedHash(newCustomer.Password);

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "INSERT INTO Customers (LoginId, Email, PhoneNumber, Password, PasswordSalt, RegisteredDate)" +
                        "VALUES(@loginId, @email, @phoneNumber, @password, @passwordSalt, @registeredDate)";

                    conn.Execute(sql,
                        new
                        {
                            loginId = newCustomer.LoginId,
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
            }
            return newCustomer;
        }

        [HttpPost]
        [Route("Login")]
        public bool Login([FromBody] Customers customer)
        {

            bool authenticated = false;
            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "SELECT Password, PasswordSalt FROM Customers WHERE LoginId = @loginId;";
                    var result = conn.Query(sql, new { loginId = customer.LoginId }).FirstOrDefault();
                    
                    if (result != null)
                    {
                        if (hasher.VerifyPassword(customer.Password, result.PasswordSalt, result.Password))
                        {
                            authenticated = true;
                        }
                    }

                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return authenticated;
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
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
