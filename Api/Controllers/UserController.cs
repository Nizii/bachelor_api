﻿using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Api.Controllers.UserController;
using BCrypt.Net;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager _userManager;
        public UserController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            var conStr = "mongodb+srv://nizamoezdemir:QdAvPanY1ql36WaR@interaktiveweinkarte.gc1ktjp.mongodb.net/test";
            var client = new MongoClient(conStr);
            var database = client.GetDatabase("ikwdb");
            _users = database.GetCollection<User>("User");
            _userManager = new UserManager(database);
        }

        [HttpGet]
        public JsonResult Get()
        {
            var dbList = _users.AsQueryable();

            return new JsonResult(dbList);
        }

        // https://localhost:44322/api/User/login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] User model)
        {
            var user = await _userManager.Get(model.Username);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!VerifyPassword(model.Password, user.Password))
            {
                return Unauthorized();
            }

            // Wenn der Benutzer authentifiziert ist, generieren Sie einen JWT-Token und senden ihn an das Frontend.
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // https://localhost:44322/api/User/reg
        [HttpPost]
        [Route("reg")]
        public async Task<IActionResult> CreateUser([FromBody] User model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest();
            }
            model.Password = HashPassword(model.Password);
            await _users.InsertOneAsync(model);

            return Ok(model);
        }


        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }

        private string GenerateJwtToken(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username))
            {
                throw new ArgumentNullException(nameof(user), "User or Username cannot be null or empty");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, user.Username),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public class UserManager
        {
            private readonly IMongoCollection<User> _users;

            public UserManager(IMongoDatabase database)
            {
                _users = database.GetCollection<User>("User");
            }
            public async Task<User> Get(string username)
            {
                var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
                return user;
            }
        }
        
    }
}
