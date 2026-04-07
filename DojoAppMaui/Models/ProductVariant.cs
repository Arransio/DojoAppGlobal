namespace DojoAppMaui.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public string Size { get; set; }
        public List<ProductVariantColor> Colors { get; set; }
    }
}
