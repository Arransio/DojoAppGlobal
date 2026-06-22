using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
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
            .Include(v => v.Colors)
                .ThenInclude(vc => vc.Color)
            .ToList();

        return Ok(variants);
    }

    // Crear variante (simple)
    [HttpPost]
    public IActionResult Create(ProductVariant variant)
    {
        _context.ProductVariants.Add(variant);
        _context.SaveChanges();

        return Ok(variant);
    }

    // Obtiene o crea una variante para un producto y talla dados
    [HttpGet("ensure/{productId}/{size}")]
    public IActionResult EnsureVariant(int productId, string size)
    {
        var productExists = _context.Products.Any(p => p.Id == productId);
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