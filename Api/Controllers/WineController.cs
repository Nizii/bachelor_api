using Api.Models;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
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

        [HttpPut("{id}")]
        public IActionResult UpdateRating(int id)
        {
            MongoClient dbClient = new MongoClient(conStr);
            var collection = dbClient.GetDatabase("ikwdb").GetCollection<Wine>("Weine");

            var wineToUpdate = collection.Find(w => w._id == id).FirstOrDefault();
            if (wineToUpdate == null)
            {
                return NotFound();
            }

            var updateDefinition = Builders<Wine>.Update.Inc(w => w.Rating, 1);
            collection.UpdateOne(w => w._id == id, updateDefinition);

            return NoContent();
        }
    }
}