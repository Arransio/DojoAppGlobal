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

	// Usuario
	private string username;
	public string Username
	{
		get => username;
		set => SetProperty(ref username, value);
	}

	// Email
	private string email;
	public string Email
	{
		get => email;
		set => SetProperty(ref email, value);
	}

	// Contraseña
	private string password;
	public string Password
	{
		get => password;
		set => SetProperty(ref password, value);
	}

	// Control de throttling para registro
	private DateTime _lastRegisterClickTime = DateTime.MinValue;
	private const int RegisterThrottleSeconds = 60; // 1 minuto

	private bool _isRegisterButtonEnabled = true;
	public bool IsRegisterButtonEnabled
	{
		get => _isRegisterButtonEnabled;
		set => SetProperty(ref _isRegisterButtonEnabled, value);
	}

	private string _registerButtonText = "¿No tienes cuenta? Registrarse";
	public string RegisterButtonText
	{
		get => _registerButtonText;
		set => SetProperty(ref _registerButtonText, value);
	}

	public ICommand LoginCommand { get; }
	public ICommand RegisterCommand { get; }

	public LoginViewModel()
	{
		_authService = new AuthService();

		LoginCommand = new Command(async () => await Login());
		RegisterCommand = new Command(async () => await OnRegisterClicked());
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

				// Guardar UserId
				if (result.UserId > 0)
				{
					await TokenStorage.SaveUserId(result.UserId);
					Debug.WriteLine($"[LoginViewModel] UserId guardado: {result.UserId}");
				}

				// Guardar Role
				if (!string.IsNullOrEmpty(result.Role))
				{
					await TokenStorage.SaveRole(result.Role);
					Debug.WriteLine($"[LoginViewModel] Role guardado: {result.Role}");
				}

			var storedToken = await TokenStorage.GetToken();
			Debug.WriteLine($"[LoginViewModel] Token guardado correctamente");

			// Navegar a HomePage
			Debug.WriteLine("[LoginViewModel] Login exitoso, navegando a HomePage");
			await Application.Current.MainPage.Navigation.PushAsync(new HomePage());

			// Limpiar campos de credenciales
			Username = string.Empty;
			Email = string.Empty;
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

	private async Task OnRegisterClicked()
	{
		// Verificar throttling (esperar 1 minuto entre intentos)
		var timeSinceLastClick = DateTime.Now - _lastRegisterClickTime;

		if (timeSinceLastClick.TotalSeconds < RegisterThrottleSeconds && _lastRegisterClickTime != DateTime.MinValue)
		{
			var secondsRemaining = RegisterThrottleSeconds - (int)timeSinceLastClick.TotalSeconds;
			await Application.Current.MainPage.DisplayAlert(
				"Espera por favor",
				$"Puedes volver a registrarte en {secondsRemaining} segundos",
				"OK");
			return;
		}

		_lastRegisterClickTime = DateTime.Now;
		IsRegisterButtonEnabled = false;
		StartThrottleTimer();

		// Pedir datos de registro
		var action = await Application.Current.MainPage.DisplayActionSheet(
			"Crear nueva cuenta",
			"Cancelar",
			null,
			"Continuar");

		if (action != "Continuar")
		{
			return;
		}

		// Mostrar diálogo de entrada para nuevo usuario
		var newUsername = await Application.Current.MainPage.DisplayPromptAsync(
			"Nuevo Usuario",
			"Ingresa un nombre de usuario (mínimo 3 caracteres)",
			placeholder: "usuario",
			maxLength: 50);

		if (string.IsNullOrWhiteSpace(newUsername))
		{
			return;
		}

		if (newUsername.Length < 3)
		{
			await Application.Current.MainPage.DisplayAlert(
				"Error",
				"El usuario debe tener mínimo 3 caracteres",
				"OK");
			return;
		}

		// Mostrar diálogo de entrada para email
		var newEmail = await Application.Current.MainPage.DisplayPromptAsync(
			"Email",
			"Ingresa tu email",
			placeholder: "correo@ejemplo.com",
			maxLength: 100);

		if (string.IsNullOrWhiteSpace(newEmail))
		{
			return;
		}

		if (!IsValidEmail(newEmail))
		{
			await Application.Current.MainPage.DisplayAlert(
				"Error",
				"Por favor ingresa un email válido",
				"OK");
			return;
		}

		// Mostrar diálogo de entrada para nueva contraseña
		var newPassword = await Application.Current.MainPage.DisplayPromptAsync(
			"Nueva Contraseña",
			"Ingresa una contraseña (mínimo 4 caracteres)",
			placeholder: "contraseña",
			maxLength: 50);

		if (string.IsNullOrWhiteSpace(newPassword))
		{
			return;
		}

		if (newPassword.Length < 4)
		{
			await Application.Current.MainPage.DisplayAlert(
				"Error",
				"La contraseña debe tener mínimo 4 caracteres",
				"OK");
			return;
		}

		// Intentar registrar
		await AttemptRegister(newUsername, newEmail, newPassword);
	}

	private async Task AttemptRegister(string newUsername, string newEmail, string newPassword)
	{
		if (IsBusy) return;

		IsBusy = true;

		try
		{
			Debug.WriteLine("[LoginViewModel] Iniciando registro...");

			var result = await _authService.RegisterAsync(newUsername, newEmail, newPassword);

			await Application.Current.MainPage.DisplayAlert(
				"Registro exitoso",
				$"Se ha enviado un email de confirmación a {newEmail}. Por favor revisa tu bandeja de entrada y haz clic en el link para confirmar tu cuenta.",
				"OK");

			// Limpiar campos
			Username = string.Empty;
			Email = string.Empty;
			Password = string.Empty;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[LoginViewModel] Error en registro: {ex.Message}");

			await Application.Current.MainPage.DisplayAlert(
				"Error al registrarse",
				ex.Message,
				"OK");
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async void StartThrottleTimer()
	{
		for (int i = RegisterThrottleSeconds; i > 0; i--)
		{
			RegisterButtonText = $"Espera... ({i}s)";
			await Task.Delay(1000);
		}

		IsRegisterButtonEnabled = true;
		RegisterButtonText = "¿No tienes cuenta? Registrarse";
	}

	private bool IsValidEmail(string email)
	{
		try
		{
			var addr = new System.Net.Mail.MailAddress(email);
			return addr.Address == email;
		}
		catch
		{
			return false;
		}
	}
}