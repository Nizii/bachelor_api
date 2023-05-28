using Api.Models;
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
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Wine> _wines;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager _userManager;
        public UserController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
            var conStr = _configuration["ConnectionString:Key"];
            var client = new MongoClient(conStr);
            var database = client.GetDatabase("ikwdb");
            _users = database.GetCollection<User>("User");
            _wines = database.GetCollection<Wine>("Weine");
            _userManager = new UserManager(database);

        }

        [HttpGet]
        public JsonResult Get()
        {
            var dbList = _users.AsQueryable();
            Console.WriteLine("Test");

            return new JsonResult(dbList);
        }

        
        [HttpGet]
        [Route("userdata")]
        public async Task<IActionResult> GetFavoriten()
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (userNameClaim == null)
                {
                    return Unauthorized();
                }
                var userName = userNameClaim.Value;
                var filter = Builders<User>.Filter.Eq(u => u.Username, userName);
                var user = await _users.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("remove-favorite/{wineId}")]
        [Route("remove-favorite/{wineId}")]
        public async Task<IActionResult> RemoveFavorite(string wineId)
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (userNameClaim == null)
                {
                    return Unauthorized();
                }
                var userName = userNameClaim.Value;
                var filter = Builders<User>.Filter.Eq(u => u.Username, userName);
                var user = await _users.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }
                var wine = await GetWeinById(wineId);
                if (wine == null)
                {
                    return NotFound("Wein nicht gefunden");
                }
                if (user.Favoriten.Any(f => f._id == int.Parse(wineId)))
                {
                    user.Favoriten.RemoveAll(f => f._id == int.Parse(wineId));
                    var update = Builders<User>.Update.Set(u => u.Favoriten, user.Favoriten);
                    await _users.UpdateOneAsync(filter, update);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpPost("add-favorite/{wineId}")]  
        [Route("add-favorite/{wineId}")]
        public async Task<IActionResult> AddFavorite(string wineId)
        {
            try
            {
                var userNameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (userNameClaim == null)
                {
                    return Unauthorized();
                }
                var userName = userNameClaim.Value;
                var filter = Builders<User>.Filter.Eq(u => u.Username, userName);
                var user = await _users.Find(filter).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }
                var wine = await GetWeinById(wineId);
                if (wine == null)
                {
                    return NotFound("Wein nicht gefunden");
                }
                if (!user.Favoriten.Any(f => f._id == int.Parse(wineId)))
                {
                    user.Favoriten.Add(wine);
                    var update = Builders<User>.Update.Set(u => u.Favoriten, user.Favoriten);
                    await _users.UpdateOneAsync(filter, update);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        private async Task<Wine> GetWeinById(string wineId)
        {
            var filter = Builders<Wine>.Filter.Eq(w => w._id, int.Parse(wineId));
            return await _wines.Find(filter).FirstOrDefaultAsync();
        }



        // https://localhost:44322/api/User/login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin model)
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

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // https://localhost:44322/api/User/reg
        [HttpPost]
        [Route("reg")]
        public async Task<IActionResult> CreateUser([FromBody] UserRegistration model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest();
            }

            // Überprüfe, ob der Benutzername bereits existiert
            var existingUser = await _users.Find(u => u.Username == model.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest("User bereits vorhanden");
            }

            // Überprüfe, ob der Benutzername bereits existiert
            var existingEmail = await _users.Find(u => u.Email == model.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return BadRequest("Email bereits vorhanden");
            }

            // Erstelle ein User-Objekt aus dem UserAuth-Objekt
            var user = CreateUserFromUserAuth(model);
            await _users.InsertOneAsync(user);

            return Ok(user);
        }

        private User CreateUserFromUserAuth(UserAuth userAuth)
        {
            return new User
            {
                Username = userAuth.Username,
                Password = HashPassword(userAuth.Password),
                Favoriten = new List<Wine>()
            };
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
            var key2 = _configuration["Jwt:Secret"].ToString();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                }),
                Issuer = "https://localhost:44322/api/user",
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
