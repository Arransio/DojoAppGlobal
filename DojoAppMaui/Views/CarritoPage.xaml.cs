using System.Diagnostics;
using DojoAppMaui.Models;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class CarritoPage : ContentPage
{
    private readonly PedidosService _pedidosService;

    public CarritoPage()
    {
        InitializeComponent();
        _pedidosService = ServiceHelper.GetService<PedidosService>();
        LoadCarrito();
    }

    private void LoadCarrito()
    {
        var items = App.CarritoService.GetItems();

        CartCollection.ItemsSource = null;
        CartCollection.ItemsSource = items.ToList();

        RefreshTotales();

        // El estado vacío sustituye a la lista cuando no hay artículos.
        EmptyCartView.IsVisible = items.Count == 0;
        CartCollection.IsVisible = items.Count > 0;
    }

    // Actualiza total y botón sin recargar la lista (las líneas se refrescan solas
    // por binding cuando cambia la cantidad).
    private void RefreshTotales()
    {
        TotalLabel.Text = $"Total: {App.CarritoService.GetTotal():F2} €";

        if (ConfirmOrderButton != null)
            ConfirmOrderButton.IsEnabled = App.CarritoService.GetCount() > 0;

        NavBar?.RefreshBadge();
    }

    private void OnIncrementClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is CartItem item)
        {
            App.CarritoService.ChangeQuantity(item, +1);
            RefreshTotales();
        }
    }

    private void OnDecrementClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is CartItem item)
        {
            App.CarritoService.ChangeQuantity(item, -1);
            RefreshTotales();
        }
    }

    private void OnRemoveClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is CartItem item)
        {
            App.CarritoService.RemoveItem(item);
            LoadCarrito();
        }
    }

    private async void OnConfirmOrderClicked(object sender, EventArgs e)
    {
        try
        {
            var items = App.CarritoService.GetItems();

            if (items.Count == 0)
            {
                await DisplayAlert("Carrito vacío", "Añade productos antes de confirmar", "OK");
                return;
            }

            // Validar que todos los items tengan talla (variante) y colores seleccionados
            var itemsIncompletos = items.Where(item =>
                item.ProductVariantId <= 0 ||
                item.PrimaryColorId <= 0 ||
                item.SecondaryColorId <= 0).ToList();
            if (itemsIncompletos.Any())
            {
                var productosAffectados = string.Join("\n• ", itemsIncompletos.Select(i => i.ProductName));
                await DisplayAlert("Selección incompleta",
                    $"Debes seleccionar talla y colores para:\n• {productosAffectados}",
                    "OK");
                return;
            }

            // La identidad y la campaña las pone el servidor (token de sesión y
            // campaña activa); aquí no se envía ninguna de las dos.

            // El pedido se asocia al nombre completo del perfil; es obligatorio tenerlo.
            var customerName = PerfilService.GetNombre()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(customerName))
            {
                await DisplayAlert("Perfil incompleto",
                    "Para hacer un pedido, es necesario completar tu perfil", "OK");
                return;
            }

            ConfirmOrderButton.IsEnabled = false;
            ConfirmOrderButton.Text = "Procesando...";

            var response = await _pedidosService.CreatePedidoAsync(items, customerName);

            App.CarritoService.Clear();
            LoadCarrito();

            ConfirmOrderButton.Text = "Confirmar Pedido";
            ConfirmOrderButton.IsEnabled = false;

            await DisplayAlert("✅ Éxito",
                $"Pedido creado exitosamente\n\nID: {response.PedidoId}\nTotal: {response.TotalPrice:F2} €",
                "OK");

            // Volver al catálogo de pedidos tras confirmar el pedido
            Controls.BottomNavBar.SetRoot(new PedidosPage());
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
