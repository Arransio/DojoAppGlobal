using System.Text.Json.Serialization;

namespace Api_Dojo_App.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public int CampaignId { get; set; }

        [JsonIgnore]
        public Campaign? Campaign { get; set; }

        public List<PedidoItem> Items { get; set; } = new();

    }
}
