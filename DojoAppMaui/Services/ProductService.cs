using System.Text.Json;
using DojoAppMaui.Models;

namespace DojoAppMaui.Services
{
	// Catálogo: productos, colores y variantes (producto + talla).
	public class ProductService
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		private readonly HttpClient _httpClient;

		// El HttpClient llega ya configurado desde MauiProgram vía IHttpClientFactory.
		public ProductService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		// Los GET lanzan si el servidor falla (EnsureSuccessStatusCode) en lugar de
		// devolver lista vacía: una lista vacía significa "no hay datos", y un fallo
		// de red debe llegar a la UI como error para poder mostrar "reintentar".
		public async Task<List<Product>> GetProducts()
		{
			var response = await _httpClient.GetAsync("api/products");
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<List<Product>>(json, JsonOptions) ?? new List<Product>();
		}

		public async Task<List<Colores>> GetColorsAsync()
		{
			var response = await _httpClient.GetAsync("api/Colors");
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<List<Colores>>(json, JsonOptions) ?? new List<Colores>();
		}

		public async Task<List<ProductVariant>> GetVariantsAsync()
		{
			var response = await _httpClient.GetAsync("api/ProductVariants");
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<List<ProductVariant>>(json, JsonOptions) ?? new List<ProductVariant>();
		}

		// Obtiene o crea la variante (producto + talla). POST: el endpoint crea datos.
		public async Task<ProductVariant?> EnsureVariantAsync(int productId, string size)
		{
			var response = await _httpClient.PostAsync($"api/ProductVariants/ensure/{productId}/{size}", null);

			if (!response.IsSuccessStatusCode)
				return null;

			var json = await response.Content.ReadAsStringAsync();

			return JsonSerializer.Deserialize<ProductVariant>(json, JsonOptions);
		}
	}
}
