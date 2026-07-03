using System.Diagnostics;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Cargar datos del usuario logueado en la cabecera
		await LoadUserInfo();

		// Avisos de pedidos pendientes de pago del usuario
		await LoadAvisosPagoAsync();
	}

	// Muestra un aviso por cada pedido del usuario que aún no esté marcado como pagado.
	private async Task LoadAvisosPagoAsync()
	{
		try
		{
			AvisosPagoContainer.Children.Clear();
			AvisosPagoContainer.IsVisible = false;
			PagosSection.IsVisible = false;
			PagosChevron.Text = "▾";

			var userId = await TokenStorage.GetUserId();
			if (userId == null || userId <= 0)
				return;

			var service = new OrderReportService();
			var pedidos = await service.GetPedidosByUserAsync(userId.Value);

			var pendientes = pedidos.Where(p => !p.EstaPagado).ToList();
			if (pendientes.Count == 0)
				return;

			foreach (var pedido in pendientes)
				AvisosPagoContainer.Children.Add(BuildAvisoPago(pedido));

			// Solo se muestra el desplegable (contraído) con el número de pendientes.
			PagosHeaderLabel.Text = $"Pedidos por pagar ({pendientes.Count})";
			PagosSection.IsVisible = true;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[HomePage] Error cargando avisos de pago: {ex.Message}");
		}
	}

	// Despliega/contrae la lista de pedidos pendientes.
	private void OnTogglePagosClicked(object sender, EventArgs e)
	{
		var abierto = !AvisosPagoContainer.IsVisible;
		AvisosPagoContainer.IsVisible = abierto;
		PagosChevron.Text = abierto ? "▴" : "▾";
	}

	private static Border BuildAvisoPago(PedidoUsuarioDto pedido)
	{
		var accent = GetColor("AccentSalmon", Colors.Salmon);

		var titulo = new Label
		{
			Text = $"Pedido id: {pedido.Id} pendiente de pago",
			FontFamily = "Sora",
			FontAttributes = FontAttributes.Bold,
			FontSize = 14,
			TextColor = accent
		};

		var detalle = new Label
		{
			Text = $"Importe: {pedido.TotalPrice:0.00} €",
			FontFamily = "Sora",
			FontSize = 12,
			TextColor = GetColor("TextSecondary", Colors.Gray)
		};

		return new Border
		{
			BackgroundColor = GetColor("GlassFill", Colors.Black),
			Stroke = new SolidColorBrush(accent),
			StrokeThickness = 1,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
			Padding = new Thickness(16, 12),
			Content = new VerticalStackLayout
			{
				Spacing = 2,
				Children = { titulo, detalle }
			}
		};
	}

	private static Color GetColor(string key, Color fallback)
	{
		if (Application.Current?.Resources?.TryGetValue(key, out var value) == true && value is Color color)
			return color;
		return fallback;
	}

	private async Task LoadUserInfo()
	{
		try
		{
			// Preferimos el nombre y apellidos del perfil; si no, el username de la sesión.
			var nombrePerfil = PerfilService.GetNombre();
			var display = !string.IsNullOrWhiteSpace(nombrePerfil)
				? nombrePerfil
				: await TokenStorage.GetUsername();

			if (string.IsNullOrWhiteSpace(display))
				display = "Invitado";

			UsernameLabel.Text = display;

			// Avatar: círculo del color del cinturón con una franja por cada grado.
			RenderBeltAvatar();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[HomePage] Error cargando usuario: {ex.Message}");
		}
	}

	private void RenderBeltAvatar()
	{
		// Si el usuario ha guardado una foto, la usamos como avatar.
		if (PerfilService.TieneFoto())
		{
			AvatarFoto.Source = ImageSource.FromFile(PerfilService.GetFotoPath());
			AvatarFoto.IsVisible = true;
			AvatarStripes.IsVisible = false;
			AvatarBorder.BackgroundColor = Colors.Transparent;
			return;
		}

		AvatarFoto.IsVisible = false;
		AvatarStripes.IsVisible = true;

		var cinturon = PerfilService.GetCinturon();
		var grados = PerfilService.GetGrado();

		AvatarBorder.BackgroundColor = PerfilService.GetCinturonColor(cinturon);

		// Cinturón blanco (o sin elegir) -> franjas negras; resto -> blancas.
		var stripeColor = PerfilService.EsCinturonBlanco(cinturon) ? Colors.Black : Colors.White;

		AvatarStripes.Children.Clear();
		for (int i = 0; i < grados; i++)
		{
			AvatarStripes.Children.Add(new BoxView
			{
				Color = stripeColor,
				BackgroundColor = stripeColor,
				HeightRequest = 3,
				HorizontalOptions = LayoutOptions.Fill
			});
		}
	}
}
