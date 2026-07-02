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
        private readonly string _baseUrl = "http://10.0.2.2:5221/api/pedidos";

        public OrderReportService()
        {
        }

        // Pedidos de un usuario concreto (para los avisos de pago en su pantalla de inicio).
        public async Task<List<PedidoUsuarioDto>> GetPedidosByUserAsync(int userId)
        {
            try
            {
                using var httpClient = new HttpClient();
                var url = $"{_baseUrl}/user/{userId}";
                Debug.WriteLine($"[OrderReportService] GET {url}");

                var response = await httpClient.GetAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error al obtener pedidos del usuario: {response.StatusCode} - {responseJson}");

                var pedidos = JsonSerializer.Deserialize<List<PedidoUsuarioDto>>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return pedidos ?? new List<PedidoUsuarioDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderReportService] Error GetPedidosByUser: {ex.Message}");
                throw;
            }
        }

        // Guarda en lote el estado de pago de varios pedidos.
        public async Task UpdatePaymentsAsync(List<PaymentUpdateDto> updates)
        {
            try
            {
                using var httpClient = new HttpClient();
                var url = $"{_baseUrl}/payments";
                var json = JsonSerializer.Serialize(updates);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                Debug.WriteLine($"[OrderReportService] POST {url} ({updates.Count} cambios)");

                var response = await httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error al guardar pagos: {response.StatusCode} - {responseJson}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OrderReportService] Error UpdatePayments: {ex.Message}");
                throw;
            }
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

                    // Debug: verificar si los colores llegaron
                    if (pedidos != null)
                    {
                        foreach (var pedido in pedidos)
                        {
                            Debug.WriteLine($"[OrderReportService] Pedido #{pedido.Id} con {pedido.Items.Count} items");
                            foreach (var item in pedido.Items)
                            {
                                Debug.WriteLine($"[OrderReportService]   - {item.ProductName} (Talla: {item.Size}, Colores recibidos: {item.Colors?.Count ?? 0})");
                                if (item.Colors != null && item.Colors.Count > 0)
                                {
                                    foreach (var color in item.Colors)
                                    {
                                        Debug.WriteLine($"[OrderReportService]     * Color: {color.Name} (Role: {color.Role})");
                                    }
                                }
                            }
                        }
                    }

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

        public bool EstaPagado =>
            string.Equals(Status, "Pagado", StringComparison.OrdinalIgnoreCase);
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
        public List<ColorDto> Colors { get; set; } = new();
    }

    public class ColorDto
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }

    // Pedido reducido de un usuario, para los avisos de pago en la pantalla de inicio.
    public class PedidoUsuarioDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool EstaPagado =>
            string.Equals(Status, "Pagado", StringComparison.OrdinalIgnoreCase);
    }

    // Cambio de estado de pago enviado desde la pantalla de Pagos.
    public class PaymentUpdateDto
    {
        public int PedidoId { get; set; }
        public bool IsPaid { get; set; }
    }
}
