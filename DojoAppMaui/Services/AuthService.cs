using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DojoAppMaui.Models;

namespace DojoAppMaui.Services
{
	public class AuthService
	{
		private readonly HttpClient _httpClient;

		public AuthService()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("http://10.0.2.2:5221/")
			};
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

			try
			{
				var response = await _httpClient.PostAsync("api/auth/login", content);

				var responseJson = await response.Content.ReadAsStringAsync();
				Debug.WriteLine($"[AuthService] Response: {responseJson}");

				if (!response.IsSuccessStatusCode) 
				{
					Debug.WriteLine($"[AuthService] Login error: {response.StatusCode} - {responseJson}");

					// Especificar el tipo de error
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						throw new Exception("Usuario o contraseña incorrectos");
					}
					else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
					{
						throw new Exception("Datos inválidos. Por favor verifica tu entrada");
					}
					else
					{
						throw new Exception($"Error del servidor: {response.StatusCode}");
					}
				}

				var result = JsonSerializer.Deserialize<LoginResponse>(
					responseJson,
					new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

				if (result?.Token != null)
				{
					Debug.WriteLine($"[AuthService] Token recibido correctamente: {result.Token.Substring(0, Math.Min(20, result.Token.Length))}...");
				}
				else
				{
					Debug.WriteLine("[AuthService] ⚠️ Token es null o vacío en la respuesta");
					throw new Exception("No se recibió token en la respuesta del servidor");
				}

				return result;
			}
			catch (HttpRequestException ex)
			{
				Debug.WriteLine($"[AuthService] Error de conexión: {ex.Message}");
				throw new Exception($"Error de conexión con el servidor: {ex.Message}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[AuthService] Exception en LoginAsync: {ex.Message}");
				throw;
			}
		}

		private async Task AddAuthorizationHeader() 
		{
			var token = await TokenStorage.GetToken();
			if (!string.IsNullOrEmpty(token)) 
			{
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}

		public async Task<string> TestAuth()
		{
			var token = await TokenStorage.GetToken();

			if (string.IsNullOrEmpty(token))
			{
				Debug.WriteLine("[AuthService] ⚠️ NO HAY TOKEN para hacer TestAuth");
				throw new Exception("No token available");
			}

			Debug.WriteLine($"[AuthService] TOKEN TEST: {token.Substring(0, Math.Min(20, token.Length))}...");

			var url = "http://10.0.2.2:5221/api/auth/profile";

			Debug.WriteLine($"[AuthService] LLAMANDO A: {url}");

			try
			{
				var request = new HttpRequestMessage(HttpMethod.Get, url);

				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

				Debug.WriteLine($"[AuthService] Authorization header: Bearer {token.Substring(0, Math.Min(20, token.Length))}...");

				var response = await _httpClient.SendAsync(request);

				Debug.WriteLine($"[AuthService] STATUS: {response.StatusCode}");

				var content = await response.Content.ReadAsStringAsync();
				Debug.WriteLine($"[AuthService] CONTENT: {content}");

				if (!response.IsSuccessStatusCode)
				{
					Debug.WriteLine($"[AuthService] ⚠️ Respuesta no exitosa. Verifique el token y la configuración JWT");
				}

				return content;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"[AuthService] ERROR HTTP: {ex.Message}");
				throw;
			}
		}
	}
}
