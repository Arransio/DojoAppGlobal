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
                }
            }
        }

        public ObservableCollection<ColorOption> AvailablePrimaryColors { get; set; } = new();

        public string? SelectedPrimaryColor
        {
            get => _selectedPrimaryColor;
            set
            {
                if (SetProperty(ref _selectedPrimaryColor, value))
                {
                    OnPropertyChanged(nameof(CanSelectSecondaryColor));
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
                }
            }
        }

        // Helpers for UI flow control
        public bool CanSelectSize => IsExpanded;

        public bool CanSelectPrimaryColor => !string.IsNullOrEmpty(SelectedSizeStep);

        public bool CanSelectSecondaryColor => !string.IsNullOrEmpty(SelectedPrimaryColor);

        public bool CanAddToCart => !string.IsNullOrEmpty(SelectedSecondaryColor);

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
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;

        public override bool Equals(object? obj) => obj is ColorOption other && other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}