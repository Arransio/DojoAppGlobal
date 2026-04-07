using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DojoAppMaui.ViewModels
{
    public class ProductVariantColorViewModel : INotifyPropertyChanged
    {
        public int ColorId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
