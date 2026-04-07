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
}