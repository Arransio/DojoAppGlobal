namespace DojoAppMaui.Services
{
    using DojoAppMaui.Models;
    // Services/ApiService.cs
    using System.Net.Http.Json;

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

        public async Task<Campaign> GetActiveCampaignAsync()
        {
            return await _httpClient.GetFromJsonAsync<Campaign>("api/campaigns/active");
        }
    }
}
