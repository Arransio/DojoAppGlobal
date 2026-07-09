using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int CampaignId { get; set; }

        // Soft-delete: los productos no se borran (destruiría el histórico de
        // pedidos), se retiran del catálogo marcándolos inactivos.
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public List<ProductVariant> ProductVariants { get; set; } = new();
    }
}
