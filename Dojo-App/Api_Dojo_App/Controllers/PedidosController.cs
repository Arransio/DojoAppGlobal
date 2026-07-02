using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public IActionResult CreatePedidoFromCart([FromBody] CreatePedidoRequest request)
    {
        try
        {
            Debug.WriteLine($"\n========== [PedidosController] INICIANDO CREACIÓN DE PEDIDO ==========");
            Debug.WriteLine($"[PedidosController] Request recibido:");
            Debug.WriteLine($"  - UserId: {request.UserId}");
            Debug.WriteLine($"  - CampaignId: {request.CampaignId}");
            Debug.WriteLine($"  - Items: {request.Items?.Count ?? 0}");
            Debug.WriteLine($"  - TotalPrice: {request.TotalPrice}");

            // Validar que request no sea null
            if (request == null)
            {
                Debug.WriteLine("[PedidosController] ❌ Request es null");
                return BadRequest(new { error = "Request no puede ser null" });
            }

            // Validar usuario
            Debug.WriteLine($"[PedidosController] Buscando usuario con ID: {request.UserId}");
            var user = _context.Users.FirstOrDefault(u => u.Id == request.UserId);

            if (user == null)
            {
                var usuariosEnBd = _context.Users.Select(u => new { u.Id, u.Username }).ToList();
                Debug.WriteLine($"[PedidosController] ❌ Usuario {request.UserId} no encontrado");
                Debug.WriteLine($"[PedidosController] Usuarios en BD: {string.Join(", ", usuariosEnBd.Select(u => $"{u.Id}({u.Username})"))}");
                return NotFound(new { error = $"Usuario con ID {request.UserId} no existe", usuariosDisponibles = usuariosEnBd });
            }
            Debug.WriteLine($"[PedidosController] ✅ Usuario encontrado: {user.Username}");

            // El pedido debe ir asociado al nombre completo del perfil del usuario.
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                Debug.WriteLine("[PedidosController] ❌ CustomerName vacío: perfil incompleto");
                return BadRequest(new { error = "Para hacer un pedido, es necesario completar tu perfil" });
            }

            // Validar campaña
            Debug.WriteLine($"[PedidosController] Buscando campaña con ID: {request.CampaignId}");
            var campaign = _context.Campaigns
                .FirstOrDefault(c => c.Id == request.CampaignId);

            if (campaign == null)
            {
                var campanasEnBd = _context.Campaigns.Select(c => new { c.Id, c.Name, c.IsActive }).ToList();
                Debug.WriteLine($"[PedidosController] ❌ Campaña {request.CampaignId} no encontrada");
                Debug.WriteLine($"[PedidosController] Campañas en BD: {string.Join(", ", campanasEnBd.Select(c => $"{c.Id}({c.Name}-Active:{c.IsActive})"))}");
                return NotFound(new { error = $"Campaña con ID {request.CampaignId} no existe", campanasDisponibles = campanasEnBd });
            }

            if (!campaign.IsActive)
            {
                Debug.WriteLine($"[PedidosController] ❌ Campaña {request.CampaignId} no activa");
                return BadRequest(new { error = "La campaña no está activa" });
            }
            Debug.WriteLine($"[PedidosController] ✅ Campaña encontrada: {campaign.Name}");

            // Validar items
            if (request.Items == null || !request.Items.Any())
            {
                Debug.WriteLine("[PedidosController] ❌ El pedido no tiene items");
                return BadRequest(new { error = "El pedido debe tener al menos un item" });
            }
            Debug.WriteLine($"[PedidosController] ✅ Validando {request.Items.Count} items...");

            // Validar y preparar items
            var pedidoItems = new List<PedidoItem>();

            foreach (var itemRequest in request.Items)
            {
                Debug.WriteLine($"[PedidosController]   - Buscando variante {itemRequest.ProductVariantId}...");

                var variant = _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefault(v => v.Id == itemRequest.ProductVariantId);

                if (variant == null)
                {
                    var variantesEnBd = _context.ProductVariants.Select(v => new { v.Id, v.ProductId, v.Size }).Take(10).ToList();
                    Debug.WriteLine($"[PedidosController] ❌ Variante {itemRequest.ProductVariantId} no existe");
                    Debug.WriteLine($"[PedidosController] Primeras 10 variantes en BD: {string.Join(", ", variantesEnBd.Select(v => $"{v.Id}(P:{v.ProductId},S:{v.Size})"))}");
                    return NotFound(new { error = $"Variante {itemRequest.ProductVariantId} no existe", variantesEjemplo = variantesEnBd });
                }

                if (itemRequest.Quantity <= 0)
                {
                    Debug.WriteLine("[PedidosController] ❌ Cantidad inválida");
                    return BadRequest(new { error = "La cantidad debe ser mayor que 0" });
                }

                // Validar colores (se eligen en el pedido, ya no en la variante)
                var primaryColor = _context.Colors.FirstOrDefault(c => c.Id == itemRequest.PrimaryColorId);
                if (primaryColor == null)
                {
                    Debug.WriteLine($"[PedidosController] ❌ Color primario {itemRequest.PrimaryColorId} no existe");
                    return NotFound(new { error = $"Color primario con ID {itemRequest.PrimaryColorId} no existe" });
                }

                var secondaryColor = _context.Colors.FirstOrDefault(c => c.Id == itemRequest.SecondaryColorId);
                if (secondaryColor == null)
                {
                    Debug.WriteLine($"[PedidosController] ❌ Color secundario {itemRequest.SecondaryColorId} no existe");
                    return NotFound(new { error = $"Color secundario con ID {itemRequest.SecondaryColorId} no existe" });
                }

                Debug.WriteLine($"[PedidosController]   ✅ Variante encontrada: {variant.Product.Name} - {variant.Size} ({primaryColor.Name}/{secondaryColor.Name})");

                var pedidoItem = new PedidoItem
                {
                    ProductVariantId = itemRequest.ProductVariantId,
                    PrimaryColorId = itemRequest.PrimaryColorId,
                    SecondaryColorId = itemRequest.SecondaryColorId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = itemRequest.UnitPrice,
                    TotalPrice = itemRequest.UnitPrice * itemRequest.Quantity
                };

                pedidoItems.Add(pedidoItem);
            }

            // Crear nuevo pedido
            Debug.WriteLine("[PedidosController] ✅ Creando pedido en BD...");
            var newPedido = new Pedido
            {
                UserId = request.UserId,
                CustomerName = request.CustomerName.Trim(),
                CampaignId = request.CampaignId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pendiente",
                Items = pedidoItems,
                TotalPrice = request.TotalPrice
            };

            _context.Pedidos.Add(newPedido);
            _context.SaveChanges();

            Debug.WriteLine($"[PedidosController] ✅✅✅ Pedido creado exitosamente con ID: {newPedido.Id}");
            Debug.WriteLine($"========== [PedidosController] FIN CREACIÓN EXITOSA ==========\n");

            return Ok(new CreatePedidoResponse
            {
                PedidoId = newPedido.Id,
                Message = "Pedido creado exitosamente",
                TotalPrice = newPedido.TotalPrice,
                CreatedAt = newPedido.CreatedAt
            });
        }
        catch (Exception ex)
        {
            // La causa real de un fallo en SaveChanges suele venir en la inner exception
            // (p. ej. errores de SQLite como "database disk image is malformed").
            var innerMessage = ex.InnerException?.Message;
            Debug.WriteLine($"[PedidosController] ❌❌❌ EXCEPCIÓN: {ex.Message}");
            if (innerMessage != null)
                Debug.WriteLine($"[PedidosController] ↳ Inner: {innerMessage}");
            Debug.WriteLine($"[PedidosController] Stack: {ex.StackTrace}");
            Debug.WriteLine($"========== [PedidosController] FIN CON ERROR ==========\n");

            return StatusCode(500, new { error = ex.Message, innerError = innerMessage, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost]
    public IActionResult CreatePedido(Pedido pedido)
    {
        // Validar campaña
        var campaign = _context.Campaigns
            .FirstOrDefault(c => c.Id == pedido.CampaignId && c.IsActive);

        if (campaign == null)
            return BadRequest("Campaña no válida o no activa");

        // Validar usuario
        var userExists = _context.Users.Any(u => u.Id == pedido.UserId);
        if (!userExists)
            return BadRequest("Usuario no válido");

        // Validar items
        if (pedido.Items == null || !pedido.Items.Any())
            return BadRequest("El pedido debe tener al menos un item");



        var newItems = new List<PedidoItem>();

        foreach (var item in pedido.Items)
        {
            var variant = _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefault(v => v.Id == item.ProductVariantId);

            if (variant == null)
                return BadRequest($"Variant {item.ProductVariantId} no existe");

            if (item.Quantity <= 0)
                return BadRequest("La cantidad debe ser mayor que 0");

            var pedidoItem = new PedidoItem
            {
                ProductVariantId = item.ProductVariantId,
                PrimaryColorId = item.PrimaryColorId,
                SecondaryColorId = item.SecondaryColorId,
                Quantity = item.Quantity,
                UnitPrice = variant.Product.Price
            };

            newItems.Add(pedidoItem);
        }

        // busco pedido existente
        var existingPedido = _context.Pedidos
            .Include(p => p.Items)
            .FirstOrDefault(p => p.UserId == pedido.UserId && p.CampaignId == pedido.CampaignId);

        if (existingPedido != null)
        {
            //borramos los viejos
            _context.PedidoItems.RemoveRange(existingPedido.Items);

            existingPedido.Items = newItems;
            existingPedido.CreatedAt = DateTime.UtcNow;

            existingPedido.TotalPrice = newItems.Sum(i => i.UnitPrice * i.Quantity);

            _context.SaveChanges();

            return Ok(existingPedido);
        }

        // Si no existe → crear nuevo
        pedido.Items = newItems;
        pedido.CreatedAt = DateTime.UtcNow;

        pedido.TotalPrice = newItems.Sum(i => i.UnitPrice * i.Quantity);

        _context.Pedidos.Add(pedido);
        _context.SaveChanges();

        return Ok(pedido);
    }

    [HttpGet("summary/{campaignId}")]
    public IActionResult GetSummary(int campaignId)
    {
        var summary = _context.PedidoItems
            .Where(pi => pi.Pedido.CampaignId == campaignId)
            .Include(pi => pi.ProductVariant)
                .ThenInclude(v => v.Product)
            .Include(pi => pi.PrimaryColor)
            .Include(pi => pi.SecondaryColor)
            .ToList()
            .GroupBy(pi => new
            {
                Product = pi.ProductVariant.Product.Name,
                Size = pi.ProductVariant.Size,
                Colors = (pi.PrimaryColor != null ? pi.PrimaryColor.Name : "?") + " (Primary), "
                       + (pi.SecondaryColor != null ? pi.SecondaryColor.Name : "?") + " (Secondary)"
            })
            .Select(g => new
            {
                g.Key.Product,
                g.Key.Size,
                g.Key.Colors,
                Total = g.Sum(x => x.Quantity)
            })
            .OrderBy(x => x.Product)
            .ThenBy(x => x.Size)
            .ToList();

        return Ok(summary);
    }

    [HttpGet("all")]
    public IActionResult GetAllPedidos()
    {
        try
        {
            Debug.WriteLine("[PedidosController] GetAllPedidos - Obteniendo todos los pedidos");

            var colorsDict = _context.Colors.ToDictionary(c => c.Id, c => c.Name);

            var pedidos = _context.Pedidos
                .Include(p => p.Items)
                    .ThenInclude(pi => pi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToList()
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    // Se identifica por el nombre de perfil; si el pedido es antiguo y no lo
                    // tiene, se recurre al username como respaldo.
                    UserName = string.IsNullOrWhiteSpace(p.CustomerName) ? p.User.Username : p.CustomerName,
                    p.CampaignId,
                    p.Status,
                    p.TotalPrice,
                    p.CreatedAt,
                    Items = p.Items.Select(item => new
                    {
                        item.Id,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice,
                        ProductName = item.ProductVariant.Product.Name,
                        ProductVariantId = item.ProductVariantId,
                        Size = item.ProductVariant.Size,
                        Colors = BuildColorList(item.PrimaryColorId, item.SecondaryColorId, colorsDict)
                    }).ToList()
                })
                .ToList();

            Debug.WriteLine($"[PedidosController] Se retornaron {pedidos.Count} pedidos");
            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PedidosController] Error al obtener pedidos: {ex.Message}");
            if (ex.InnerException != null)
                Debug.WriteLine($"[PedidosController] ↳ Inner: {ex.InnerException.Message}");
            return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    // Pedidos de un usuario concreto. Se usa para mostrar avisos de pago pendiente
    // en la pantalla de inicio del propio usuario.
    [HttpGet("user/{userId}")]
    public IActionResult GetPedidosByUser(int userId)
    {
        try
        {
            var pedidos = _context.Pedidos
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Status,
                    p.TotalPrice,
                    p.CreatedAt
                })
                .ToList();

            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PedidosController] Error al obtener pedidos del usuario {userId}: {ex.Message}");
            return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    // Actualiza en lote el estado de pago de varios pedidos (marcar/desmarcar "Pagado").
    // Lo usa el administrador desde la pantalla de Pagos.
    [HttpPost("payments")]
    public IActionResult UpdatePayments([FromBody] List<PaymentUpdate> updates)
    {
        try
        {
            if (updates == null || updates.Count == 0)
                return BadRequest(new { error = "No hay cambios que guardar" });

            var ids = updates.Select(u => u.PedidoId).ToList();
            var pedidos = _context.Pedidos.Where(p => ids.Contains(p.Id)).ToList();

            foreach (var pedido in pedidos)
            {
                var update = updates.First(u => u.PedidoId == pedido.Id);
                pedido.Status = update.IsPaid ? "Pagado" : "Pendiente";
            }

            _context.SaveChanges();

            Debug.WriteLine($"[PedidosController] Estado de pago actualizado en {pedidos.Count} pedidos");
            return Ok(new { updated = pedidos.Count });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PedidosController] Error al actualizar pagos: {ex.Message}");
            return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message });
        }
    }

    private static List<ColorEntry> BuildColorList(int primaryId, int secondaryId, Dictionary<int, string> colorsDict)
    {
        var list = new List<ColorEntry>();
        if (primaryId > 0 && colorsDict.TryGetValue(primaryId, out var primaryName))
            list.Add(new ColorEntry { Name = primaryName, Role = "Primary" });
        if (secondaryId > 0 && colorsDict.TryGetValue(secondaryId, out var secondaryName))
            list.Add(new ColorEntry { Name = secondaryName, Role = "Secondary" });
        return list;
    }
}

public class ColorEntry
{
    public string Name { get; set; }
    public string Role { get; set; }
}

// Cambio de estado de pago de un pedido enviado desde la pantalla de Pagos.
public class PaymentUpdate
{
    public int PedidoId { get; set; }
    public bool IsPaid { get; set; }
}
