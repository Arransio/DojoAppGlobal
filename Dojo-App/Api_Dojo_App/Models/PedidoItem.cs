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

        // Colores elegidos en el momento del pedido (antes vivían en ProductVariantColor).
        public int PrimaryColorId { get; set; }

        [JsonIgnore]
        public Color? PrimaryColor { get; set; }

        public int SecondaryColorId { get; set; }

        [JsonIgnore]
        public Color? SecondaryColor { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
