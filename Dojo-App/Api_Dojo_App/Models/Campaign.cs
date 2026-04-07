namespace Api_Dojo_App.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
        public List<Product> Products { get; set; } = new();
        public List<Pedido> Pedidos { get; set; } = new();

    }
}
