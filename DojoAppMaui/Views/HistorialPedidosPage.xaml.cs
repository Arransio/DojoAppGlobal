using DojoAppMaui.Services;
using DojoAppMaui.ViewModels;

namespace DojoAppMaui.Views;

// Vista "tonta": todo el estado (carga, error, vacío, datos) vive en el ViewModel.
// El code-behind solo conecta el BindingContext y la navegación.
public partial class HistorialPedidosPage : ContentPage
{
    private readonly HistorialPedidosViewModel _viewModel;

    public HistorialPedidosPage()
    {
        InitializeComponent();
        _viewModel = ServiceHelper.GetService<HistorialPedidosViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // CargarAsync captura sus propias excepciones: este async void no puede lanzar.
        await _viewModel.CargarAsync();
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
