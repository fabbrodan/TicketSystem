using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Utils;
using TicketSystemAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace TicketSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IOptions<DbOptions> _dbOptions;

        public AdminController(IOptions<DbOptions> dbOptions)
        {
            _dbOptions = dbOptions;
        }

        [HttpPost]
        [Route("Login")]
        public bool Login([FromBody]Administrators admin)
        {
            var authenticated = false;
            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var sql = "SELECT Password, PasswordSalt FROM Administrators WHERE LoginId = @loginId;";
                    var result = conn.Query(sql, new { loginId = admin.LoginId }).FirstOrDefault();

                    if (hasher.VerifyPassword(admin.Password, result.PasswordSalt, result.Password))
                    {
                        authenticated = true;
                    }
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return authenticated;
        }

        [HttpPost]
        [Route("NewAdmin")]
        public Administrators NewAdmin([FromBody] Administrators admin)
        {
            Administrators newAdmin = admin;
            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();
            newAdmin.PasswordSalt = hasher.RandomSalt;
            newAdmin.Password = hasher.GenerateSaltedHash(admin.Password);
            newAdmin.RegisteredDate = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var sql = "INSERT INTO Administrators (LoginId, Email, Password, PasswordSalt, RegisteredDate)" +
                        "VALUES(@LoginId, @Email, @Password, @PasswordSalt, @RegisteredDate);";

                    conn.Execute(sql, newAdmin);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }
            return newAdmin;
        }
    }
}