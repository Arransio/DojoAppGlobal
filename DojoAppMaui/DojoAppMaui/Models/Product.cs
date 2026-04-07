namespace DojoAppMaui.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductVariant SelectedVariant { get; set; }
        public List<ProductVariant> Variants { get; set; }
    }
}
