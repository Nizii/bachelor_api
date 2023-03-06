using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend
{
    public class BackendApi : ServiceBase<BackendApi>
    {
        private readonly ILogger<BackendApi> _logger;
        private readonly IMongoCollection<Wein> _weinCollection;

        public BackendApi(ILogger<BackendApi> logger)
        {
            _logger = logger;

            // Hier wird die Verbindung zur Datenbank aufgebaut.
            //var client = new MongoClient("mongodb://localhost:27017");
            var client = new MongoClient("mongodb+srv://nizamoezdemir:QdAvPanY1ql36WaR@interaktiveweinkarte.gc1ktjp.mongodb.net/test");
            var database = client.GetDatabase("ikwdb");
            _weinCollection = database.GetCollection<Wein>("Weine");
        }

        public override async Task<WeinReply> GetWein(WeinRequest request, ServerCallContext context)
        {
            var wein = await _weinCollection.Find(x => x.Id == request.Id).FirstOrDefaultAsync();
            if (wein == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Wein mit der ID {request.Id} wurde nicht gefunden."));
            }

            return new WeinReply
            {
                Id = wein.Id,
                Name = wein.Name,
                Art = wein.Art,
                Herkunft = wein.Herkunft,
                Alkohol = wein.Alkohol,
                Traubenart = wein.Traubenart,
                Flaschenpreis = wein.Flaschenpreis,
                Offenpreis = wein.Offenpreis
            };
        }
    }

    public class Wein
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Art { get; set; }
        public string Herkunft { get; set; }
        public int Alkohol { get; set; }
        public string Traubenart { get; set; }
        public int Flaschenpreis { get; set; }
        public int Offenpreis { get; set; }
    }
}
