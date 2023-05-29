using MongoDB.Bson;

namespace Api.Models
{
    public class Wine
    {
        private const string PHOTO_URL = "https://wine.azurewebsites.net/Imgs/";
        //private const string PHOTO_URL = "https://localhost:44322/Imgs/";
        public int _id { get; set; }
        public string Name { get; set; }
        public string Winetype { get; set; }
        public string Origin { get; set; }
        public string Grape { get; set; }
        public decimal Alcohol { get; set; }
        public decimal Openprice { get; set; }
        public decimal Bottleprice { get; set; }
        public int Selected { get; set; }
        public string Charakter { get; set; }
        public string MatchWith { get; set; }
        public string ServingTemperature { get; set; }

        public string StorageTime { get; set; }
        public int Year { get; set; }
        public string[] Match { get; set; }
        public string[] FoodTags { get; set; }
        public string[] GrapeTags { get; set; }
        public string NationTag { get; set; }
        public string RegionTag { get; set; }
        public string[] CharacterTags { get; set; }

        public string[][] Comments { get; set; }
        public string[] Profile { get; set; }

        public int Rating { get; set; }

        public string Winzer { get; set; }

        public int[] radarchart { get; set; }


        public string Link { get { return PHOTO_URL + _id.ToString()+ ".png"; } }
    }
}