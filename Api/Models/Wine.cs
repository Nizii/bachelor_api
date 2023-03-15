namespace Api.Models
{
    public class Wine
    {
        private const string PHOTO_URL = "interactivemenu.azurewebsites.net/photos";
        //private const string PHOTO_URL = "https://localhost:44322/Photos";
        public int _id { get; set; }
        public string Name { get; set; }
        public string Winetype { get; set; }
        public string Origin { get; set; }
        public string Grape { get; set; }
        public decimal Alcohol { get; set; }
        public decimal Openprice { get; set; }
        public decimal Bottleprice { get; set; }
        public int Selected { get; set; }
        public string Link { get { return PHOTO_URL + Name; } }
    }
}