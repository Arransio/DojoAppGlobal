namespace Api_Dojo_App.Models
{
    public class CreatePedidoRequest
    {
        public int UserId { get; set; }
        public int CampaignId { get; set; }
        public List<PedidoItemRequest> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class PedidoItemRequest
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreatePedidoResponse
    {
        public int PedidoId { get; set; }
        public string Message { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
