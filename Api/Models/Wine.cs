namespace Api.Models
{
    public class Wine
    {
        public int _id { get; set; }
        public string Name { get; set; }
        public string Weinart { get; set; }
        public string Herkunft { get; set; }
        public string Traubenart { get; set; }
        public decimal Alkoholgehalt { get; set; }
        public decimal Offenpreis { get; set; }
        public decimal Flaschenpreis { get; set; }
        public int Ausgewählt { get; set; }
    }
}