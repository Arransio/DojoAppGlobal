using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace DojoAppMaui.Services
{
    public class OrderReportService
    {
        private readonly string _apiUrl = "http://10.0.2.2:5221/api/pedidos/all";

        public OrderReportService()
        {
        }

        public async Task<List<PedidoDto>> GetAllPedidosAsync()
        {
            try
            {
                Debug.WriteLine("[OrderReportService] Obteniendo todos los pedidos");

                using (var httpClient = new HttpClient())
                {
                    Debug.WriteLine($"[OrderReportService] Enviando GET a: {_apiUrl}");

                    var response = await httpClient.GetAsync(_apiUrl);

                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[OrderReportService] Response Status: {response.StatusCode}");

                    if (responseJson.Length > 200)
                        Debug.WriteLine($"[OrderReportService] Response: {responseJson.Substring(0, 200)}...");
                    else
                        Debug.WriteLine($"[OrderReportService] Response: {responseJson}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"[OrderReportService] Error: {response.StatusCode}");
                        throw new Exception($"Error al obtener pedidos: {response.StatusCode} - {responseJson}");
                    }

                    var pedidos = JsonSerializer.Deserialize<List<PedidoDto>>(
                        responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    Debug.WriteLine($"[OrderReportService] Se obtuvieron {pedidos?.Count ?? 0} pedidos");
                    return pedidos ?? new List<PedidoDto>();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[OrderReportService] Error de conexión: {ex.Message}");
                throw new Exception($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderReportService] Error: {ex.Message}");
                throw;
            }
        }
    }

    // DTOs
    public class PedidoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CampaignId { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PedidoItemDto> Items { get; set; } = new();
    }

    public class PedidoItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ProductName { get; set; }
        public int ProductVariantId { get; set; }
        public string Size { get; set; }
    }
}
