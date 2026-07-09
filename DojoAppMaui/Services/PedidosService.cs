using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DojoAppMaui.Models;

namespace DojoAppMaui.Services
{
    public class PedidosService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        // El HttpClient llega ya configurado desde MauiProgram vía IHttpClientFactory.
        public PedidosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // La identidad NO se envía: el servidor la toma del token (evita suplantación).
        // Los precios tampoco: se calculan en el servidor desde la BD.
        public async Task<CreatePedidoResponse> CreatePedidoAsync(List<CartItem> items, int campaignId, string customerName)
        {
            Debug.WriteLine($"[PedidosService] Creando pedido con {items.Count} items");

            // Validar que todos los items tengan variante (talla) y colores seleccionados
            var itemsIncompletos = items.Where(item =>
                item.ProductVariantId <= 0 ||
                item.PrimaryColorId <= 0 ||
                item.SecondaryColorId <= 0).ToList();
            if (itemsIncompletos.Any())
            {
                var productosAffectados = string.Join(", ", itemsIncompletos.Select(i => i.Product.Name));
                throw new Exception($"Los siguientes productos no tienen talla o color seleccionados: {productosAffectados}");
            }

            var pedidoItems = items.Select(item => new PedidoItemRequest
            {
                ProductVariantId = item.ProductVariantId,
                PrimaryColorId = item.PrimaryColorId,
                SecondaryColorId = item.SecondaryColorId,
                Quantity = 1
            }).ToList();

            var request = new CreatePedidoRequestMAUI
            {
                CustomerName = customerName,
                CampaignId = campaignId,
                Items = pedidoItems
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Debug.WriteLine($"[PedidosService] POST api/pedidos/create: CampaignId={campaignId}, Items={pedidoItems.Count}");

            var response = await _httpClient.PostAsync("api/pedidos/create", content);

            var responseJson = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[PedidosService] Response: {responseJson}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al crear pedido: {response.StatusCode} - {responseJson}");

            var result = JsonSerializer.Deserialize<CreatePedidoResponse>(responseJson, JsonOptions);

            Debug.WriteLine($"[PedidosService] Pedido creado exitosamente con ID: {result?.PedidoId}");
            return result;
        }
    }

    // DTOs locales en MAUI. Reflejan el contrato del servidor: sin UserId
    // (sale del token) y sin precios (se calculan server-side).
    public class CreatePedidoRequestMAUI
    {
        public string CustomerName { get; set; } = string.Empty;
        public int CampaignId { get; set; }
        public List<PedidoItemRequest> Items { get; set; } = new();
    }

    public class PedidoItemRequest
    {
        public int ProductVariantId { get; set; }
        public int PrimaryColorId { get; set; }
        public int SecondaryColorId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreatePedidoResponse
    {
        public int PedidoId { get; set; }
        public string Message { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
