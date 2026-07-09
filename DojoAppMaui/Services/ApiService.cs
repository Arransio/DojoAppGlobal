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
			// AuthHttpHandler añade el token y gestiona los 401 de sesión caducada
			_httpClient = new HttpClient(new AuthHttpHandler())
			{
				BaseAddress = new Uri(baseUrl)
			};
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
