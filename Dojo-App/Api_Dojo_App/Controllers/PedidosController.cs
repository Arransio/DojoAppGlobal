using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
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
}