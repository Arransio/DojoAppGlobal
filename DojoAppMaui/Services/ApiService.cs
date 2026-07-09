namespace DojoAppMaui.Services
{
	using DojoAppMaui.Models;
	using System.Net.Http.Json;

	public class ApiService
	{
		private readonly HttpClient _httpClient;

		// El HttpClient llega ya configurado desde MauiProgram vía IHttpClientFactory:
		// URL base (AppConfig), timeout y AuthHttpHandler (token + 401).
		public ApiService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<Campaign> GetActiveCampaignAsync()
		{
			return await _httpClient.GetFromJsonAsync<Campaign>("api/campaigns/active");
		}

		public async Task<T> GetAsync<T>(string endpoint)
		{
			return await _httpClient.GetFromJsonAsync<T>(endpoint);
		}
	}
}
