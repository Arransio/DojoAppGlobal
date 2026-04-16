namespace DojoAppMaui.Services
{
	using DojoAppMaui.Models;
	using System.Net.Http.Json;
	using System.Net.Http.Headers;
	using System.Diagnostics;

	public class ApiService
	{

#if ANDROID
		public const string baseUrl = "http://10.0.2.2:5221/";
#else
		public const string baseUrl = "https://localhost:7088/";
#endif
		private readonly HttpClient _httpClient;

		public ApiService()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(baseUrl)
			};
		}

		private async Task EnsureAuthorization()
		{
			var token = await TokenStorage.GetToken();
			if (!string.IsNullOrEmpty(token))
			{
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				Debug.WriteLine($"[ApiService] Token añadido a headers: {token.Substring(0, Math.Min(20, token.Length))}...");
			}
			else
			{
				Debug.WriteLine("[ApiService] No hay token disponible");
			}
		}

		public async Task<Campaign> GetActiveCampaignAsync()
		{
			await EnsureAuthorization();
			return await _httpClient.GetFromJsonAsync<Campaign>("api/campaigns/active");
		}

			public async Task<T> GetAsync<T>(string endpoint)
			{
				await EnsureAuthorization();
				return await _httpClient.GetFromJsonAsync<T>(endpoint);
			}
			}
		}
