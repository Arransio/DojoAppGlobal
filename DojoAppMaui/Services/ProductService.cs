using System.Text.Json;
using DojoAppMaui.Models;

public class ProductService
{
    private readonly HttpClient _httpClient;
 
    public ProductService()
    {
        _httpClient = new HttpClient();
      
    }

    public async Task<List<Product>> GetProducts()
    {
        var url = "http://10.0.2.2:5221/api/products";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return new List<Product>();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}