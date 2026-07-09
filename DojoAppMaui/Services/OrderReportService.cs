using System.Diagnostics;
using System.Text.Json;

namespace DojoAppMaui.Services
{
    public class OrderReportService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        // El HttpClient llega ya configurado desde MauiProgram vía IHttpClientFactory
        // (URL base, timeout y AuthHttpHandler: estos endpoints requieren sesión).
        public OrderReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Pedidos de un usuario concreto (para los avisos de pago en su pantalla de inicio).
        public async Task<List<PedidoUsuarioDto>> GetPedidosByUserAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/pedidos/user/{userId}");
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener pedidos del usuario: {response.StatusCode} - {responseJson}");

            var pedidos = JsonSerializer.Deserialize<List<PedidoUsuarioDto>>(responseJson, JsonOptions);

            return pedidos ?? new List<PedidoUsuarioDto>();
        }

        // Guarda en lote el estado de pago de varios pedidos.
        public async Task UpdatePaymentsAsync(List<PaymentUpdateDto> updates)
        {
            var json = JsonSerializer.Serialize(updates);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            Debug.WriteLine($"[OrderReportService] POST api/pedidos/payments ({updates.Count} cambios)");

            var response = await _httpClient.PostAsync("api/pedidos/payments", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al guardar pagos: {response.StatusCode} - {responseJson}");
        }

        public async Task<List<PedidoDto>> GetAllPedidosAsync()
        {
            var response = await _httpClient.GetAsync("api/pedidos/all");
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener pedidos: {response.StatusCode} - {responseJson}");

            var pedidos = JsonSerializer.Deserialize<List<PedidoDto>>(responseJson, JsonOptions);

            Debug.WriteLine($"[OrderReportService] Se obtuvieron {pedidos?.Count ?? 0} pedidos");

            return pedidos ?? new List<PedidoDto>();
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
