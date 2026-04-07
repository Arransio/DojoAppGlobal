namespace DojoAppMaui.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }
        public string Size { get; set; }

        public List<ProductVariantColorViewModel> Colors { get; set; }
    }
}
