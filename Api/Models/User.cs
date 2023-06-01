using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }
        
        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Favoriten")]
        public List<Wine> Favoriten { get; set; } = new List<Wine>();

        [BsonElement("Radarchart")]
        public int[] Radarchart { get; set; }
    }
}
