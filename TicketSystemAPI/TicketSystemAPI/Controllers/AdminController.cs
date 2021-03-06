﻿using System;
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

        [HttpGet("{id}")]
        public Administrators Get(int id)
        {
            Administrators returnAdmin = null;
            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM Administrators WHERE AdminId = @adminId;";
                    returnAdmin = conn.Query<Administrators>(sql, new { adminId = id }).FirstOrDefault();
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return returnAdmin;
        }

        [HttpPost]
        [Route("Login")]
        public Administrators Login([FromBody]Administrators admin)
        {
            Administrators returnAdmin = null;
            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var sql = "SELECT Password, PasswordSalt FROM Administrators WHERE LoginName = @loginName;";
                    var result = conn.Query(sql, new { admin.LoginName }).FirstOrDefault();
                    
                    if (hasher.VerifyPassword(admin.Password, result.PasswordSalt, result.Password))
                    {
                        var getSql = "SELECT * FROM Administrators WHERE LoginName = @LoginName;"; ;
                        returnAdmin = conn.Query<Administrators>(getSql, new { admin.LoginName }).FirstOrDefault();
                    }
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return returnAdmin;
        }

        [HttpPost]
        [Route("NewAdmin")]
        public Administrators NewAdmin([FromBody] Administrators admin)
        {
            Administrators newAdmin = admin;
            Administrators returnAdmin = null;
            PasswordHasher.PasswordHasher hasher = new PasswordHasher.PasswordHasher();
            newAdmin.PasswordSalt = hasher.RandomSalt;
            newAdmin.Password = hasher.GenerateSaltedHash(admin.Password);
            newAdmin.RegisteredDate = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(_dbOptions.Value.ConnectionString))
            {
                try
                {
                    conn.Open();
                    var sql = "INSERT INTO Administrators (LoginName, Email, Password, PasswordSalt, RegisteredDate)" +
                        "VALUES(@LoginName, @Email, @Password, @PasswordSalt, @RegisteredDate);";

                    conn.Execute(sql, newAdmin);
                }
                catch (SqlException exc)
                {
                    Console.WriteLine(exc.Message);
                }

                var getSql = "SELECT * FROM Administrators WHERE LoginName = @loginName;";
                returnAdmin = conn.Query<Administrators>(getSql, new { newAdmin.LoginName }).FirstOrDefault();
            }
            return returnAdmin;
        }
    }
}