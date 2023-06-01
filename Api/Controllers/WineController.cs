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

        [HttpPost("{wineId}/comments")]
        public async Task<IActionResult> AddComment(string wineId, [FromBody] Comment commentModel)
        {
            var client = new MongoClient(conStr);
            var database = client.GetDatabase("ikwdb");
            var wines = database.GetCollection<Wine>("Weine");
            var filter = Builders<Wine>.Filter.Eq(w => w._id, int.Parse(wineId));
            var wine = await wines.Find(filter).FirstOrDefaultAsync();
            if (wine == null)
            {
                return NotFound();
            }

            var commentArray = new[] { commentModel.author, commentModel.content };
            var update = Builders<Wine>.Update.Push(w => w.Comments, commentArray);
            await wines.UpdateOneAsync(filter, update);
            return Ok();
        }

    }
}