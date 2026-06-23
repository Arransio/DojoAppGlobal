
using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }

        // Una variante representa únicamente Producto + Talla.
        // El color ya NO forma parte de la variante: se elige en el pedido (PedidoItem).
        public string Size { get; set; } = string.Empty;
    }
}
