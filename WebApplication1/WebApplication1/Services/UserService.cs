using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IUserService
    {
        User Authentzicate(string username, string password);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
         new User { Id = 0, FirstName = "", LastName = "", Username = "admin", Password = "admin" }
        };

        private readonly AppSettings _appSettings;
        private readonly StackExchange.Redis.IDatabase _database;
        public UserService(IOptions<AppSettings> appSettings, StackExchange.Redis.IDatabase database)
        {
            _appSettings = appSettings.Value;
            _database = database;
        }

        public User Authentzicate(string username, string password)
        {
            string sqlConnectionString = @"Data Source = ERP-CONGLT\SQLEXPRESS; Initial Catalog = QuanLySanPham; Integrated Security = True";
            var connection = new SqlConnection(sqlConnectionString);
            List<User> listuser = new List<User>();
            connection.Open();
            listuser = connection.Query<User>("Select username,password,type from Login").ToList();
            connection.Close();

            var user = listuser.SingleOrDefault(x => x.Username == username && x.Password == password);
            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Role, user.Type),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            string fullbearer = tokenHandler.WriteToken(token);
            var allHash = _database.ListRightPush("mylist", fullbearer);
            return user.WithoutPassword();
        }

        public IEnumerable<User> GetAll()
        {
            return _users.WithoutPasswords();
        }
    }
}
