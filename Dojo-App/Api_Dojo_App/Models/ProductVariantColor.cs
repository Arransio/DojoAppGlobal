

using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class ProductVariantColor
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }

        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }

        public int ColorId { get; set; }

        [JsonIgnore]
        public Color? Color { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
