using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DojoAppMaui.Models;

namespace DojoAppMaui.Services
{
    public class PedidosService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://10.0.2.2:5221/api/pedidos/create";

        public PedidosService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<CreatePedidoResponse> CreatePedidoAsync(List<CartItem> items, int userId, int campaignId)
        {
            try
            {
                Debug.WriteLine($"[PedidosService] Creando pedido con {items.Count} items");

                // Validar que todos los items tengan una variante seleccionada
                var itemsSinVariante = items.Where(item => item.Product.SelectedVariant?.Id == null || item.Product.SelectedVariant.Id <= 0).ToList();
                if (itemsSinVariante.Any())
                {
                    var productosAffectados = string.Join(", ", itemsSinVariante.Select(i => i.Product.Name));
                    Debug.WriteLine($"[PedidosService] ❌ Los siguientes productos no tienen variante seleccionada: {productosAffectados}");
                    throw new Exception($"Los siguientes productos no tienen variante seleccionada: {productosAffectados}");
                }

                // Preparar items del pedido
                var pedidoItems = items.Select(item => new PedidoItemRequest
                {
                    ProductVariantId = item.Product.SelectedVariant.Id,
                    Quantity = 1,
                    UnitPrice = (decimal)item.Product.Price
                }).ToList();

                // Calcular total
                decimal totalPrice = (decimal)items.Sum(item => item.Product.Price);

                // Crear request
                var request = new CreatePedidoRequestMAUI
                {
                    UserId = userId,
                    CampaignId = campaignId,
                    Items = pedidoItems,
                    TotalPrice = totalPrice
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Obtener token y agregar al header
                var token = await TokenStorage.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                Debug.WriteLine($"[PedidosService] POST a: {_apiUrl}");
                Debug.WriteLine($"[PedidosService] Request: UserId={userId}, CampaignId={campaignId}, Items={pedidoItems.Count}, Total={totalPrice}€");

                var response = await _httpClient.PostAsync(_apiUrl, content);

                var responseJson = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[PedidosService] Response: {responseJson}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[PedidosService] Error: {response.StatusCode}");
                    throw new Exception($"Error al crear pedido: {response.StatusCode} - {responseJson}");
                }

                var result = JsonSerializer.Deserialize<CreatePedidoResponse>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Debug.WriteLine($"[PedidosService] Pedido creado exitosamente con ID: {result?.PedidoId}");
                return result;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[PedidosService] Error de conexión: {ex.Message}");
                throw new Exception($"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PedidosService] Error: {ex.Message}");
                throw;
            }
        }
    }

    // DTOs locales en MAUI
    public class CreatePedidoRequestMAUI
    {
        public int UserId { get; set; }
        public int CampaignId { get; set; }
        public List<PedidoItemRequest> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class PedidoItemRequest
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreatePedidoResponse
    {
        public int PedidoId { get; set; }
        public string Message { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
