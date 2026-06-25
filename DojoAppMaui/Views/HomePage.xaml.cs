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
