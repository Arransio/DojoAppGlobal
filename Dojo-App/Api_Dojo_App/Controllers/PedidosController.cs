using System.Security.Claims;
using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(AppDbContext context, ILogger<PedidosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("create")]
    public IActionResult CreatePedidoFromCart([FromBody] CreatePedidoRequest request)
    {
        // La identidad sale SIEMPRE del token firmado, nunca del body:
        // un cliente podría enviar cualquier UserId y crear pedidos a nombre de otro.
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var userId))
            return Unauthorized(new { error = "Token sin identificador de usuario" });

        _logger.LogInformation("Creando pedido: UserId={UserId}, CampaignId={CampaignId}, Items={Items}",
            userId, request.CampaignId, request.Items?.Count ?? 0);

        // Validar usuario
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            _logger.LogWarning("Usuario {UserId} del token no existe en BD al crear pedido", userId);
            return NotFound(new { error = "Usuario no válido" });
        }

        // El pedido debe ir asociado al nombre completo del perfil del usuario.
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return BadRequest(new { error = "Para hacer un pedido, es necesario completar tu perfil" });

        if (request.CustomerName.Trim().Length > 100)
            return BadRequest(new { error = "El nombre es demasiado largo (máximo 100 caracteres)" });

        // Validar campaña
        var campaign = _context.Campaigns.FirstOrDefault(c => c.Id == request.CampaignId);
        if (campaign == null)
        {
            _logger.LogWarning("Campaña {CampaignId} no encontrada al crear pedido", request.CampaignId);
            return NotFound(new { error = "Campaña no válida" });
        }

        if (!campaign.IsActive)
            return BadRequest(new { error = "La campaña no está activa" });

        // Validar items
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { error = "El pedido debe tener al menos un item" });

        // Validar y preparar items. El precio SIEMPRE se toma de la BD,
        // nunca del valor que envía el cliente.
        var pedidoItems = new List<PedidoItem>();

        foreach (var itemRequest in request.Items)
        {
            var variant = _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefault(v => v.Id == itemRequest.ProductVariantId);

            if (variant == null)
            {
                _logger.LogWarning("Variante {VariantId} no existe al crear pedido", itemRequest.ProductVariantId);
                return NotFound(new { error = $"Variante {itemRequest.ProductVariantId} no existe" });
            }

            if (itemRequest.Quantity <= 0)
                return BadRequest(new { error = "La cantidad debe ser mayor que 0" });

            // Validar colores (se eligen en el pedido, ya no en la variante)
            var primaryColor = _context.Colors.FirstOrDefault(c => c.Id == itemRequest.PrimaryColorId);
            if (primaryColor == null)
                return NotFound(new { error = $"Color primario con ID {itemRequest.PrimaryColorId} no existe" });

            var secondaryColor = _context.Colors.FirstOrDefault(c => c.Id == itemRequest.SecondaryColorId);
            if (secondaryColor == null)
                return NotFound(new { error = $"Color secundario con ID {itemRequest.SecondaryColorId} no existe" });

            var unitPrice = variant.Product.Price;

            var pedidoItem = new PedidoItem
            {
                ProductVariantId = itemRequest.ProductVariantId,
                PrimaryColorId = itemRequest.PrimaryColorId,
                SecondaryColorId = itemRequest.SecondaryColorId,
                Quantity = itemRequest.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = unitPrice * itemRequest.Quantity
            };

            pedidoItems.Add(pedidoItem);
        }

        // Crear nuevo pedido con el total calculado en servidor
        var newPedido = new Pedido
        {
            UserId = userId,
            CustomerName = request.CustomerName.Trim(),
            CampaignId = request.CampaignId,
            CreatedAt = DateTime.UtcNow,
            Status = "Pendiente",
            Items = pedidoItems,
            TotalPrice = pedidoItems.Sum(i => i.TotalPrice)
        };

        _context.Pedidos.Add(newPedido);
        _context.SaveChanges();

        _logger.LogInformation("Pedido {PedidoId} creado para usuario {UserId} (total {Total})",
            newPedido.Id, newPedido.UserId, newPedido.TotalPrice);

        return Ok(new CreatePedidoResponse
        {
            PedidoId = newPedido.Id,
            Message = "Pedido creado exitosamente",
            TotalPrice = newPedido.TotalPrice,
            CreatedAt = newPedido.CreatedAt
        });
    }

    // Resumen de producción por producto/talla/color (solo admin)
    [Authorize(Roles = "admin")]
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

    // Todos los pedidos con sus items (solo admin: reporte y pantalla de pagos)
    [Authorize(Roles = "admin")]
    [HttpGet("all")]
    public IActionResult GetAllPedidos()
    {
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

        _logger.LogInformation("GetAllPedidos: {Count} pedidos", pedidos.Count);
        return Ok(pedidos);
    }

    // Pedidos de un usuario concreto. Se usa para mostrar avisos de pago pendiente
    // en la pantalla de inicio del propio usuario. Solo el dueño (o un admin)
    // puede consultarlos.
    [Authorize]
    [HttpGet("user/{userId}")]
    public IActionResult GetPedidosByUser(int userId)
    {
        if (!IsOwnerOrAdmin(userId))
            return Forbid();

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

    // Actualiza en lote el estado de pago de varios pedidos (marcar/desmarcar "Pagado").
    // Lo usa el administrador desde la pantalla de Pagos.
    [Authorize(Roles = "admin")]
    [HttpPost("payments")]
    public IActionResult UpdatePayments([FromBody] List<PaymentUpdate> updates)
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

        _logger.LogInformation("Estado de pago actualizado en {Count} pedidos por {User}",
            pedidos.Count, User.Identity?.Name);
        return Ok(new { updated = pedidos.Count });
    }

    // El token debe pertenecer al usuario consultado, salvo que sea admin.
    private bool IsOwnerOrAdmin(int userId)
    {
        if (User.IsInRole("admin"))
            return true;

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var tokenUserId) && tokenUserId == userId;
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
