using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace DojoAppMaui.Services
{
    public class DebugAuthService
    {
        private readonly string _apiUrl = "http://10.0.2.2:5221/api/auth/profile";

        public async Task<ProfileDebugDto> GetProfileDebugAsync()
        {
            try
            {
                Debug.WriteLine("[DebugAuthService] Obteniendo perfil para debugging");

                // Obtener token
                var token = await TokenStorage.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("No hay token de autenticación.");
                }

                var role = await TokenStorage.GetRole();
                Debug.WriteLine($"[DebugAuthService] Rol guardado en cliente: {role}");
                Debug.WriteLine($"[DebugAuthService] Token: {token.Substring(0, Math.Min(50, token.Length))}...");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(_apiUrl);
                    var responseJson = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine($"[DebugAuthService] Response Status: {response.StatusCode}");
                    Debug.WriteLine($"[DebugAuthService] Response: {responseJson}");

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error: {response.StatusCode} - {responseJson}");
                    }

                    var profile = JsonSerializer.Deserialize<ProfileDebugDto>(
                        responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return profile;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugAuthService] Error: {ex.Message}");
                throw;
            }
        }
    }

    public class ProfileDebugDto
    {
        public string Message { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public bool IsAdmin { get; set; }
        public List<ClaimDto> AllClaims { get; set; } = new();
    }

    public class ClaimDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
