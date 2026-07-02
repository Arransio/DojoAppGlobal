using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        // Nombre completo elegido por el usuario en su perfil en el momento del pedido.
        // Es el nombre con el que se identifica el pedido en reportes y pagos.
        public string CustomerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public int CampaignId { get; set; }

        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public List<PedidoItem> Items { get; set; } = new();

    }
}
