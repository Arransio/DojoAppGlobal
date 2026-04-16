using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using DojoAppMaui.Views;


//using AndroidX.Browser.Trusted;
using DojoAppMaui.Services;
using DojoAppMaui.Views;


namespace DojoAppMaui.ViewModels;

public class LoginViewModel : BaseViewModel
{
	private readonly AuthService _authService;

	//Usuario
	private string username;
	public string Username
	{
		get => username;
		set => SetProperty(ref username, value);
	}

	//Contraseña
	private string password;
	public string Password
	{
		get => password;
		set => SetProperty(ref password, value);
	}

	public ICommand LoginCommand { get; }

	public LoginViewModel()
	{
		_authService = new AuthService();

		LoginCommand = new Command(async () => await Login());
	}

	private async Task Login()
	{
		if (IsBusy) return;

		IsBusy = true;

		try
		{
			// Validar que los campos no estén vacíos
			if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
			{
				await Application.Current.MainPage.DisplayAlert(
					"Error de validación",
					"Por favor ingresa usuario y contraseña",
					"OK");
				return;
			}

			Debug.WriteLine("[LoginViewModel] Iniciando login...");

			var result = await _authService.LoginAsync(Username, Password);

			if (result?.Token == null)
			{
				await Application.Current.MainPage.DisplayAlert(
					"Error de autenticación",
					"La respuesta del servidor no contiene token",
					"OK");
				return;
			}

			Debug.WriteLine($"[LoginViewModel] Token recibido: {result.Token.Substring(0, Math.Min(20, result.Token.Length))}...");

			// Guardar token
			await TokenStorage.SaveToken(result.Token);

			var storedToken = await TokenStorage.GetToken();
			Debug.WriteLine($"[LoginViewModel] Token guardado correctamente");

			// Navegar a HomePage
			Debug.WriteLine("[LoginViewModel] Login exitoso, navegando a HomePage");
			await Application.Current.MainPage.Navigation.PushAsync(new HomePage());

			// Limpiar campos de credenciales
			Username = string.Empty;
			Password = string.Empty;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[LoginViewModel] Error en login: {ex.Message}");

			// Mostrar mensaje de error específico
			if (ex.Message.Contains("401") || ex.Message.Contains("Usuario o contraseña"))
			{
				await Application.Current.MainPage.DisplayAlert(
					"Acceso denegado",
					"Usuario o contraseña incorrectos",
					"OK");
			}
			else if (ex.Message.Contains("conexión") || ex.Message.Contains("connection"))
			{
				await Application.Current.MainPage.DisplayAlert(
					"Error de conexión",
					"No se pudo conectar con el servidor. Verifica tu conexión a internet",
					"OK");
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert(
					"Error",
					$"Error al iniciar sesión: {ex.Message}",
					"OK");
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

}