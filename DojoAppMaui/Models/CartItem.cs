namespace DojoAppMaui.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public string Size { get; set; }

        // Variante (talla) y colores elegidos en el momento de añadir al carrito.
        public int ProductVariantId { get; set; }
        public int PrimaryColorId { get; set; }
        public int SecondaryColorId { get; set; }
        public string PrimaryColorName { get; set; }
        public string SecondaryColorName { get; set; }
    }
}
