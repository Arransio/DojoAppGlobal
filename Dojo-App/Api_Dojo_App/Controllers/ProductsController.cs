using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
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

    // Obtener todos los productos
    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _context.Products
            .Include(p => p.ProductVariants)
            .ToList();

        return Ok(products);
    }

    // Obtener productos por campaña
    [HttpGet("by-campaign/{campaignId}")]
    public IActionResult GetByCampaign(int campaignId)
    {
        var products = _context.Products
            .Where(p => p.CampaignId == campaignId)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.Colors)
            .ToList();

        return Ok(products);
    }

    // Crear producto
    [HttpPost]
    public IActionResult Create(Product product)
    {
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
            .ThenInclude(v => v.Colors)
            .FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // Eliminar producto
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        _context.SaveChanges();

        return NoContent();
    }
}