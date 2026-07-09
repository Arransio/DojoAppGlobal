namespace Api_Dojo_App.Models
{
    // El DTO solo declara lo que el servidor acepta del cliente.
    // Ni UserId (sale del token) ni precios (se calculan en servidor) forman parte
    // del contrato: si el cliente los envía, se ignoran al deserializar.
    public class CreatePedidoRequest
    {
        public int CampaignId { get; set; }
        public List<PedidoItemRequest> Items { get; set; } = new();

        // Nombre completo del perfil del usuario que realiza el pedido.
        public string CustomerName { get; set; } = string.Empty;
    }

    public class PedidoItemRequest
    {
        public int ProductVariantId { get; set; }
        public int PrimaryColorId { get; set; }
        public int SecondaryColorId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreatePedidoResponse
    {
        public int PedidoId { get; set; }
        public string Message { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
