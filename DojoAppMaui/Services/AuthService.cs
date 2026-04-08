using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
				BaseAddress = new Uri("http://10.0.2.2:5221")
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

			var response = await _httpClient.PostAsync("api/auth/login", content);



			if (!response.IsSuccessStatusCode) 
			{
				var error = await response.Content.ReadAsStringAsync();
				throw new Exception($"Error : {response.StatusCode} - {error}");
			}

			var responseJson = await response.Content.ReadAsStringAsync();
			Debug.WriteLine(responseJson);

			return JsonSerializer.Deserialize<LoginResponse>(responseJson);
		}
	}
}
