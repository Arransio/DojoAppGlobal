using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductVariantsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductVariantsController(AppDbContext context)
    {
        _context = context;
    }

    // Obtener todas las variantes
    [HttpGet]
    public IActionResult GetAll()
    {
        var variants = _context.ProductVariants
            .Include(v => v.Product)
            .ToList();

        return Ok(variants);
    }

    // Crear variante (solo admin)
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(CreateVariantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Size))
            return BadRequest(new { error = "La talla es requerida" });

        if (!_context.Products.Any(p => p.Id == request.ProductId))
            return NotFound(new { error = "Producto no válido" });

        var variant = new ProductVariant { ProductId = request.ProductId, Size = request.Size.Trim() };

        _context.ProductVariants.Add(variant);
        _context.SaveChanges();

        return Ok(variant);
    }

    // Obtiene o crea una variante para un producto y talla dados.
    // POST porque crea datos (un GET debe ser seguro: cachés y prefetchers pueden
    // repetirlo por su cuenta). No se restringe a admin: el flujo normal de la app
    // lo invoca cuando un usuario selecciona una talla que aún no tiene variante.
    [Authorize]
    [HttpPost("ensure/{productId}/{size}")]
    public IActionResult EnsureVariant(int productId, string size)
    {
        var productExists = _context.Products.Any(p => p.Id == productId && p.IsActive);
        if (!productExists)
            return NotFound(new { error = $"Producto {productId} no existe" });

        var variant = _context.ProductVariants
            .FirstOrDefault(v => v.ProductId == productId && v.Size == size);

        if (variant == null)
        {
            variant = new ProductVariant { ProductId = productId, Size = size };
            _context.ProductVariants.Add(variant);
            _context.SaveChanges();
        }

        return Ok(variant);
    }
}
