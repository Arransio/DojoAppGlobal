using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DojoAppMaui.Models
{
    public class Product : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private string? _selectedSizeStep;
        private string? _selectedPrimaryColor;
        private string? _selectedSecondaryColor;
        private ProductVariantUI? _selectedVariant;

        public int Id { get; set; }
        public string Name { get; set; }

        public double Price { get; set; }

        public List<string> Sizes { get; set; }

        public string SelectedSize { get; set; }

        public List<ProductVariantUI> VariantsUI { get; set; } = new();

        // Properties for Step-by-Step Selection Flow
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    OnPropertyChanged(nameof(CanSelectSize));
                    OnPropertyChanged(nameof(IsSelectingSize));
                }
            }
        }

        public ObservableCollection<string> AvailableSizes { get; set; } = new();

        public string? SelectedSizeStep
        {
            get => _selectedSizeStep;
            set
            {
                if (SetProperty(ref _selectedSizeStep, value))
                {
                    OnPropertyChanged(nameof(CanSelectPrimaryColor));
                    OnPropertyChanged(nameof(IsSelectingSize));
                    OnPropertyChanged(nameof(IsSelectingPrimaryColor));
                }
            }
        }

        // IDs de los colores elegidos (se mandan al pedido).
        public int SelectedPrimaryColorId { get; set; }
        public int SelectedSecondaryColorId { get; set; }

        public ObservableCollection<ColorOption> AvailablePrimaryColors { get; set; } = new();

        public string? SelectedPrimaryColor
        {
            get => _selectedPrimaryColor;
            set
            {
                if (SetProperty(ref _selectedPrimaryColor, value))
                {
                    OnPropertyChanged(nameof(CanSelectSecondaryColor));
                    OnPropertyChanged(nameof(IsSelectingPrimaryColor));
                    OnPropertyChanged(nameof(IsSelectingSecondaryColor));
                }
            }
        }

        public ObservableCollection<ColorOption> AvailableSecondaryColors { get; set; } = new();

        public string? SelectedSecondaryColor
        {
            get => _selectedSecondaryColor;
            set
            {
                if (SetProperty(ref _selectedSecondaryColor, value))
                {
                    OnPropertyChanged(nameof(CanAddToCart));
                    OnPropertyChanged(nameof(IsSelectingSecondaryColor));
                }
            }
        }

        // Helpers for UI flow control
        public bool CanSelectSize => IsExpanded;

        public bool CanSelectPrimaryColor => !string.IsNullOrEmpty(SelectedSizeStep);

        public bool CanSelectSecondaryColor => !string.IsNullOrEmpty(SelectedPrimaryColor);

        public bool CanAddToCart => !string.IsNullOrEmpty(SelectedSecondaryColor);

        // Accordion state: content visible only while actively selecting that step
        public bool IsSelectingSize => IsExpanded && string.IsNullOrEmpty(SelectedSizeStep);
        public bool IsSelectingPrimaryColor => !string.IsNullOrEmpty(SelectedSizeStep) && string.IsNullOrEmpty(SelectedPrimaryColor);
        public bool IsSelectingSecondaryColor => !string.IsNullOrEmpty(SelectedPrimaryColor) && string.IsNullOrEmpty(SelectedSecondaryColor);

        // Store selected variant for cart
        public ProductVariantUI? SelectedVariant
        {
            get => _selectedVariant;
            set => SetProperty(ref _selectedVariant, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class ColorOption
    {
        private static readonly Dictionary<string, string> _colorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "rojo",        "#F44336" },
            { "azul",        "#2196F3" },
            { "verde",       "#4CAF50" },
            { "negro",       "#212121" },
            { "blanco",      "#FFFFFF" },
            { "amarillo",    "#FFEB3B" },
            { "naranja",     "#FF9800" },
            { "morado",      "#9C27B0" },
            { "violeta",     "#673AB7" },
            { "rosa",        "#FFC1CC" },
            { "fucsia",      "#E91E63" },
            { "gris",        "#9E9E9E" },
            { "marron",      "#795548" },
            { "marrón",      "#795548" },
            { "celeste",     "#87CEEB" },
            { "azul marino", "#001F5B" },
            { "plateado",    "#B0BEC5" },
            { "dorado",      "#FFC107" },
            { "beige",       "#F5DEB3" },
            { "turquesa",    "#009688" },
            { "lima",        "#CDDC39" },
            { "crema",       "#FFFDD0" },
            { "coral",       "#FF7F7F" },
            { "salmon",      "#FA8072" },
            { "salmón",      "#FA8072" },
        };

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string HexColor =>
            _colorMap.TryGetValue(Name.Trim(), out var hex) ? hex : "#9E9E9E";

        public Microsoft.Maui.Graphics.Color DisplayColor =>
            Microsoft.Maui.Graphics.Color.FromArgb(HexColor);

        public override string ToString() => Name;

        public override bool Equals(object? obj) => obj is ColorOption other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}