using DojoAppMaui.Services;
using DojoAppMaui.Views;

namespace DojoAppMaui.Controls;

public partial class BottomNavBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(
            nameof(ActiveTab), typeof(string), typeof(BottomNavBar),
            default(string), propertyChanged: OnActiveTabChanged);

    /// <summary>
    /// Pestaña activa: "Inicio", "Usuario", "Pedidos", "Reporte" o "Carrito".
    /// Cada vista la fija en su XAML para resaltar el icono correspondiente.
    /// </summary>
    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public BottomNavBar()
    {
        InitializeComponent();
        ApplyActiveState();
        RefreshBadge();
        _ = ApplyAdminGatingAsync();
    }

    /// <summary>
    /// Actualiza el badge con el nº de items del carrito. Llamar tras añadir/quitar items.
    /// </summary>
    public void RefreshBadge()
    {
        var count = App.CarritoService?.GetCount() ?? 0;
        CartBadge.IsVisible = count > 0;
        CartBadgeLabel.Text = count > 99 ? "99+" : count.ToString();
    }

    // Oculta el icono de Reporte si el usuario no es admin (test1), igual que la lógica anterior.
    private async Task ApplyAdminGatingAsync()
    {
        try
        {
            var role = await TokenStorage.GetRole();
            var isAdmin = string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase);

            ItemReporte.IsVisible = isAdmin;
            // Colapsamos la columna para que no quede un hueco entre iconos.
            NavGrid.ColumnDefinitions[3].Width = isAdmin ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }
        catch
        {
            // Ante cualquier fallo, lo dejamos oculto por seguridad.
            ItemReporte.IsVisible = false;
            NavGrid.ColumnDefinitions[3].Width = new GridLength(0);
        }
    }

    private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((BottomNavBar)bindable).ApplyActiveState();
    }

    private void ApplyActiveState()
    {
        foreach (var (tab, container, icon) in EnumerateItems())
        {
            var isActive = tab == ActiveTab;
            // Todos los iconos comparten el color salmón; el activo va a opacidad plena con glow,
            // los inactivos en el mismo salmón atenuado.
            container.Opacity = isActive ? 1.0 : 0.4;
            icon.Shadow = isActive ? BuildGlow() : null;
        }
    }

    private IEnumerable<(string Tab, View Container, Microsoft.Maui.Controls.Shapes.Path Icon)> EnumerateItems()
    {
        yield return ("Inicio", ItemInicio, IconInicio);
        yield return ("Usuario", ItemUsuario, IconUsuario);
        yield return ("Pedidos", ItemPedidos, IconPedidos);
        yield return ("Reporte", ItemReporte, IconReporte);
        yield return ("Carrito", ItemCarrito, IconCarrito);
    }

    // Glow salmón para el icono activo.
    private static Shadow BuildGlow()
    {
        var color = GetColor("AccentSalmon", Colors.Salmon);
        return new Shadow
        {
            Brush = new SolidColorBrush(color),
            Radius = 10,
            Opacity = 0.85f,
            Offset = new Point(0, 0)
        };
    }

    private static Color GetColor(string key, Color fallback)
    {
        if (Application.Current?.Resources?.TryGetValue(key, out var value) == true && value is Color color)
            return color;
        return fallback;
    }

    private async void OnTabTapped(object sender, TappedEventArgs e)
    {
        var tab = e.Parameter as string;
        if (string.IsNullOrEmpty(tab) || tab == ActiveTab)
            return;

        switch (tab)
        {
            // Inicio: horario/tips. Pedidos: catálogo y alta de pedidos.
            case "Inicio":
                SetRoot(new HomePage());
                break;
            case "Pedidos":
                SetRoot(new PedidosPage());
                break;
            case "Usuario":
                SetRoot(new UsuarioPage());
                break;
            case "Carrito":
                SetRoot(new CarritoPage());
                break;
            case "Reporte":
                await OpenReporteAsync();
                break;
        }
    }

    /// <summary>
    /// Reemplaza la raíz de navegación para que no se acumule la pila al cambiar de pestaña.
    /// </summary>
    public static void SetRoot(Page page)
    {
        if (Application.Current != null)
            Application.Current.MainPage = new NavigationPage(page);
    }

    private async Task OpenReporteAsync()
    {
        var current = Application.Current?.MainPage;
        if (current == null)
            return;

        try
        {
            var orderReportService = new OrderReportService();
            var pedidos = await orderReportService.GetAllPedidosAsync();

            if (pedidos.Count == 0)
            {
                await current.DisplayAlert("Sin pedidos", "No hay pedidos para mostrar", "OK");
                return;
            }

            var pdfService = new PdfGeneratorService();
            var html = pdfService.GenerateHtmlPreview(pedidos);
            SetRoot(new ReportePreviewPage(html, pedidos));
        }
        catch (Exception ex)
        {
            await current.DisplayAlert("Error", $"Error al cargar el reporte: {ex.Message}", "OK");
        }
    }
}
