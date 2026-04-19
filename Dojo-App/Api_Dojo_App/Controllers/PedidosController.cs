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

                Debug.WriteLine($"[PedidosController]   ✅ Variante encontrada: {variant.Product.Name} - {variant.Size}");

                var pedidoItem = new PedidoItem
                {
                    ProductVariantId = itemRequest.ProductVariantId,
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
            Debug.WriteLine($"[PedidosController] ❌❌❌ EXCEPCIÓN: {ex.Message}");
            Debug.WriteLine($"[PedidosController] Stack: {ex.StackTrace}");
            Debug.WriteLine($"========== [PedidosController] FIN CON ERROR ==========\n");

            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
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
            .Include(pi => pi.ProductVariant)
                .ThenInclude(v => v.Colors)
                    .ThenInclude(vc => vc.Color)
            .ToList()
            .GroupBy(pi => new
            {
                Product = pi.ProductVariant.Product.Name,
                Size = pi.ProductVariant.Size,
                Colors = string.Join(", ",
                    pi.ProductVariant.Colors
                        .Select(c => c.Color.Name + " (" + c.Role + ")"))
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
                    UserName = p.User.Username,
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
                        Size = item.ProductVariant.Size
                    }).ToList()
                })
                .ToList();

            Debug.WriteLine($"[PedidosController] Se retornaron {pedidos.Count} pedidos");
            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PedidosController] Error al obtener pedidos: {ex.Message}");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}