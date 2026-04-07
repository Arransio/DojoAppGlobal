
using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
        public string Size { get; set; } = string.Empty;

        public List<ProductVariantColor> Colors { get; set; } = new();

    }
}
