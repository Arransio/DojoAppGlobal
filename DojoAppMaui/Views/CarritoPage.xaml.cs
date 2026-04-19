using System.Diagnostics;
using System.Linq;
using DojoAppMaui.Models;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class CarritoPage : ContentPage
{
    private List<CartItem> cart;
    private PedidosService _pedidosService;

    public CarritoPage()
    {
        InitializeComponent();
        _pedidosService = new PedidosService();
        LoadCarrito();
    }

    private void LoadCarrito()
    {
        var items = App.CarritoService.GetItems();

        CartCollection.ItemsSource = null;
        CartCollection.ItemsSource = items.ToList();

        double total = App.CarritoService.GetTotal();
        TotalLabel.Text = $"Total: {total:F2} €";

        // Habilitar/deshabilitar botón de confirmar
        if (ConfirmOrderButton != null)
            ConfirmOrderButton.IsEnabled = items.Count > 0;
    }

    private void OnRemoveClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var item = button?.BindingContext as CartItem;

        if (item != null)
        {
            App.CarritoService.RemoveItem(item);
            LoadCarrito();
        }
    }

    private async void OnConfirmOrderClicked(object sender, EventArgs e)
    {
        try
        {
            Debug.WriteLine("[CarritoPage] Iniciando confirmación de pedido");

            var items = App.CarritoService.GetItems();

            if (items.Count == 0)
            {
                await DisplayAlert("Carrito vacío", "Añade productos antes de confirmar", "OK");
                return;
            }

            // Validar que todos los items tengan variante seleccionada
            var itemsSinVariante = items.Where(item => item.Product.SelectedVariant == null || item.Product.SelectedVariant.Id <= 0).ToList();
            if (itemsSinVariante.Any())
            {
                var productosAffectados = string.Join("\n• ", itemsSinVariante.Select(i => i.Product.Name));
                await DisplayAlert("Selección incompleta", 
                    $"Debes seleccionar una variante (talla/color) para:\n• {productosAffectados}", 
                    "OK");
                return;
            }

            // Obtener UserId
            var userId = await TokenStorage.GetUserId();
            if (userId == null || userId <= 0)
            {
                await DisplayAlert("Error", "No se encontró el usuario. Por favor, inicia sesión nuevamente", "OK");
                return;
            }

            Debug.WriteLine($"[CarritoPage] UserId: {userId}");

            // TODO: Obtener la campaña activa - por ahora usamos 1 como default
            int campaignId = 1;

            ConfirmOrderButton.IsEnabled = false;
            ConfirmOrderButton.Text = "Procesando...";

            var response = await _pedidosService.CreatePedidoAsync(items, userId.Value, campaignId);

            // Limpiar carrito
            App.CarritoService.GetItems().Clear();
            LoadCarrito();

            ConfirmOrderButton.Text = "Confirmar Pedido";
            ConfirmOrderButton.IsEnabled = false;

            await DisplayAlert("✅ Éxito", 
                $"Pedido creado exitosamente\n\nID: {response.PedidoId}\nTotal: {response.TotalPrice:F2} €", 
                "OK");

            // Volver a la página anterior
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CarritoPage] Error: {ex.Message}");
            ConfirmOrderButton.IsEnabled = true;
            ConfirmOrderButton.Text = "Confirmar Pedido";

            await DisplayAlert("Error", $"Error al crear pedido: {ex.Message}", "OK");
        }
    }
}