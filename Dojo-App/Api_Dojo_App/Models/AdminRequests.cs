namespace Api_Dojo_App.Models
{
    // DTOs de entrada para los endpoints de administración de catálogo.
    // Declaran exactamente lo que el servidor acepta: recibir la entidad EF
    // directa permitiría al cliente fijar Ids o grafos anidados arbitrarios
    // (over-posting / mass-assignment).

    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CampaignId { get; set; }
    }

    public class CreateCampaignRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateColorRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class CreateVariantRequest
    {
        public int ProductId { get; set; }
        public string Size { get; set; } = string.Empty;
    }
}
