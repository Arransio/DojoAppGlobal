using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DojoAppMaui.Models
{
    // Línea del carrito. Es un modelo PLANO (solo los datos elegidos), no arrastra
    // el Product completo: así se puede serializar a Preferences para que el carrito
    // sobreviva a que Android mate el proceso en segundo plano.
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity = 1;

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double UnitPrice { get; set; }

        // Variante (talla) y colores elegidos en el momento de añadir al carrito.
        public string Size { get; set; } = string.Empty;
        public int ProductVariantId { get; set; }
        public int PrimaryColorId { get; set; }
        public int SecondaryColorId { get; set; }
        public string PrimaryColorName { get; set; } = string.Empty;
        public string SecondaryColorName { get; set; } = string.Empty;

        // Observable: el stepper +/- del carrito actualiza la línea sin recargar la lista.
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value)
                    return;
                _quantity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPrice)));
            }
        }

        // Calculado: no se persiste (se deriva de precio y cantidad).
        [JsonIgnore]
        public double TotalPrice => UnitPrice * Quantity;

        // Dos líneas son "el mismo artículo" si coinciden variante y colores:
        // en ese caso no se duplica la línea, se suma cantidad.
        public bool EsMismoArticulo(CartItem other) =>
            other != null
            && ProductVariantId == other.ProductVariantId
            && PrimaryColorId == other.PrimaryColorId
            && SecondaryColorId == other.SecondaryColorId;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
