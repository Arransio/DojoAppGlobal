using System.Diagnostics;
using DojoAppMaui.Services;
using DojoAppMaui.ViewModels;

namespace DojoAppMaui.Views;

public partial class LoginPage : ContentPage
{
	// Evita repetir la comprobación si la página reaparece (p. ej. al volver de un push)
	private bool _autoLoginChecked;

	public LoginPage()
	{
		InitializeComponent();
		BindingContext = new LoginViewModel();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (_autoLoginChecked)
			return;
		_autoLoginChecked = true;

		await TryAutoLoginAsync();
	}

	// Auto-login: si hay un JWT guardado y aún no ha caducado, entra directo a Home.
	// Se hace aquí (y no en App.OnStart) porque reemplazar MainPage durante el arranque
	// puede dejar la UI congelada en Android. El token demo no es un JWT
	// (GetTokenExpiryUtc devuelve null), así que el modo demo siempre pasa por el login.
	// Si el token resulta inválido en el servidor, AuthHttpHandler detectará el 401.
	private async Task TryAutoLoginAsync()
	{
		try
		{
			var token = await TokenStorage.GetToken();
			if (string.IsNullOrEmpty(token))
				return;

			var expiry = TokenStorage.GetTokenExpiryUtc(token);
			if (expiry == null || expiry <= DateTime.UtcNow.AddMinutes(1))
				return;

			Debug.WriteLine($"[LoginPage] Auto-login: token válido hasta {expiry:u}");
			Application.Current.MainPage = new NavigationPage(new HomePage());
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[LoginPage] Error en auto-login: {ex.Message}");
		}
	}
}
