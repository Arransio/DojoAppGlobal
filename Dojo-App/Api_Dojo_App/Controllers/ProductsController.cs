using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // Obtener todos los productos (solo los activos: los retirados no salen del catálogo)
    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.ProductVariants)
            .ToList();

        return Ok(products);
    }

    // Obtener productos por campaña
    [HttpGet("by-campaign/{campaignId}")]
    public IActionResult GetByCampaign(int campaignId)
    {
        var products = _context.Products
            .Where(p => p.CampaignId == campaignId && p.IsActive)
            .Include(p => p.ProductVariants)
            .ToList();

        return Ok(products);
    }

    // Crear producto (solo admin)
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "El nombre es requerido" });

        if (request.Price < 0)
            return BadRequest(new { error = "El precio no puede ser negativo" });

        if (!_context.Campaigns.Any(c => c.Id == request.CampaignId))
            return NotFound(new { error = "Campaña no válida" });

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            CampaignId = request.CampaignId
        };

        _context.Products.Add(product);
        _context.SaveChanges();

        return Ok(product);
    }

    // Obtener producto por id
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _context.Products
            .Include(p => p.ProductVariants)
            .FirstOrDefault(p => p.Id == id && p.IsActive);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // Retirar producto del catálogo (solo admin).
    // Soft-delete: borrar la fila destruiría en cascada las líneas de pedidos
    // históricos que referencian sus variantes. Se marca inactivo y deja de
    // aparecer en el catálogo, pero el histórico queda íntegro.
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound();

        product.IsActive = false;
        _context.SaveChanges();

        return NoContent();
    }
}
