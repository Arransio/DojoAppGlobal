using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DojoAppMaui.Models;

namespace DojoAppMaui.Services
{
	public class AuthService
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		private readonly HttpClient _httpClient;

		// El HttpClient llega ya configurado desde MauiProgram vía IHttpClientFactory.
		// SIN AuthHttpHandler a propósito: aquí un 401 es "credenciales incorrectas",
		// no "sesión caducada", y no debe disparar el logout global.
		// Los errores de red (HttpRequestException, TaskCanceledException por timeout)
		// se dejan subir tal cual para que la UI los distinga por TIPO, no por mensaje.
		public AuthService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<LoginResponse> LoginAsync(string username, string password)
		{
			var loginData = new
			{
				Username = username,
				Password = password
			};

			var json = JsonSerializer.Serialize(loginData);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("api/auth/login", content);

			var responseJson = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				Debug.WriteLine($"[AuthService] Login error: {response.StatusCode}");

				if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					throw new Exception("Usuario o contraseña incorrectos");

				if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
					throw new Exception("Datos inválidos. Por favor verifica tu entrada");

				throw new Exception($"Error del servidor: {response.StatusCode}");
			}

			var result = JsonSerializer.Deserialize<LoginResponse>(responseJson, JsonOptions);

			if (result?.Token == null)
				throw new Exception("No se recibió token en la respuesta del servidor");

			return result;
		}

		public async Task<RegisterResponse> RegisterAsync(string username, string email, string password)
		{
			var registerData = new
			{
				Username = username,
				Email = email,
				Password = password
			};

			var json = JsonSerializer.Serialize(registerData);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("api/auth/register", content);

			var responseJson = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				Debug.WriteLine($"[AuthService] Register error: {response.StatusCode} - {responseJson}");

				if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
					throw new Exception("Por favor verifica: usuario (mín 3 caracteres), contraseña (mín 8 caracteres), email válido");

				throw new Exception($"Error al registrarse: {response.StatusCode}");
			}

			var result = JsonSerializer.Deserialize<RegisterResponse>(responseJson, JsonOptions);

			Debug.WriteLine($"[AuthService] Registro exitoso. Se envió email de confirmación a: {email}");

			return result;
		}

		public async Task<bool> ConfirmEmailAsync(string email, string token)
		{
			var confirmData = new
			{
				Email = email,
				Token = token
			};

			var json = JsonSerializer.Serialize(confirmData);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("api/auth/confirm-email", content);

			if (!response.IsSuccessStatusCode)
			{
				Debug.WriteLine($"[AuthService] Confirm email error: {response.StatusCode}");
				throw new Exception("No se pudo confirmar el email. El token puede ser inválido o estar expirado.");
			}

			Debug.WriteLine($"[AuthService] Email confirmado exitosamente para: {email}");
			return true;
		}
	}
}
