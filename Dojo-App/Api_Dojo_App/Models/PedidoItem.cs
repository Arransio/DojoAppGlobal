using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class PedidoItem
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [JsonIgnore]
        public Pedido? Pedido { get; set; }

        public int ProductVariantId { get; set; }

        [JsonIgnore]
        public ProductVariant? ProductVariant { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

