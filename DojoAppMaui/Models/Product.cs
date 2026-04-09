using System.Text.Json.Serialization;

namespace DojoAppMaui.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public double Price { get; set; }

        public List<string> Sizes { get; set; }

        public string SelectedSize { get; set; }
    }
}