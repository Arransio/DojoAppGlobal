using System.Diagnostics;
using System.Text.Json;
using DojoAppMaui.Models;

namespace DojoAppMaui.Services;

// Estado del carrito. Reglas de la clase:
// - La lista interna NO se expone: GetItems() devuelve IReadOnlyList, y toda
//   mutación pasa por métodos explícitos (AddItem, Remove, Clear...). Así, si un
//   día "el carrito se vació solo", solo hay un sitio donde mirar.
// - Cada cambio se persiste a Preferences como JSON: Android mata procesos en
//   segundo plano con normalidad y el carrito debe sobrevivir a eso.
// - Añadir un artículo ya existente (misma variante y colores) suma cantidad
//   en vez de duplicar la línea.
public class CarritoService
{
    private const string PreferencesKey = "carrito_items";

    private readonly List<CartItem> cart;

    public CarritoService()
    {
        cart = LoadFromPreferences();
    }

    public IReadOnlyList<CartItem> GetItems() => cart;

    public void AddItem(CartItem item)
    {
        var existente = cart.FirstOrDefault(x => x.EsMismoArticulo(item));

        if (existente != null)
            existente.Quantity += item.Quantity;
        else
            cart.Add(item);

        Save();
    }

    public void RemoveItem(CartItem item)
    {
        cart.Remove(item);
        Save();
    }

    public void Clear()
    {
        cart.Clear();
        Save();
    }

    // Cambia la cantidad de una línea en +1/-1. Bajar de 1 no elimina la línea:
    // quitar del carrito es una acción explícita (el botón de borrar).
    public void ChangeQuantity(CartItem item, int delta)
    {
        if (!cart.Contains(item))
            return;

        item.Quantity = Math.Max(1, item.Quantity + delta);
        Save();
    }

    public double GetTotal()
    {
        return cart.Sum(x => x.TotalPrice);
    }

    // Total de unidades (no de líneas): 2 kimonos + 1 cinturón = 3.
    public int GetCount()
    {
        return cart.Sum(x => x.Quantity);
    }

    // --- Persistencia ---

    private void Save()
    {
        try
        {
            Preferences.Set(PreferencesKey, JsonSerializer.Serialize(cart));
        }
        catch (Exception ex)
        {
            // El carrito sigue funcionando en memoria; solo falla la copia local.
            Debug.WriteLine($"[CarritoService] No se pudo persistir el carrito: {ex.Message}");
        }
    }

    private static List<CartItem> LoadFromPreferences()
    {
        try
        {
            var json = Preferences.Get(PreferencesKey, null);
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }
        catch (Exception ex)
        {
            // JSON corrupto o de una versión antigua del modelo: se descarta.
            Debug.WriteLine($"[CarritoService] Carrito guardado ilegible, se empieza vacío: {ex.Message}");
        }

        return new List<CartItem>();
    }
}
