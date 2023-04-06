using Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WineController : ControllerBase
    {
        private string conStr = "mongodb+srv://nizamoezdemir:QdAvPanY1ql36WaR@interaktiveweinkarte.gc1ktjp.mongodb.net/test";
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public WineController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public JsonResult Get()
        {
            MongoClient dbClient = new MongoClient(conStr);

            var dbList = dbClient.GetDatabase("ikwdb").GetCollection<Wine>("Weine").AsQueryable();

            return new JsonResult(dbList);
        }
    }
}