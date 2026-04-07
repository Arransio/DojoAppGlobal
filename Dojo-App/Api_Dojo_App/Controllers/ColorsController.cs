using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ColorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ColorsController(AppDbContext context)
    {
        _context = context;
    }

    // Crear color
    [HttpPost]
    public IActionResult Create(Color color)
    {
        _context.Colors.Add(color);
        _context.SaveChanges();

        return Ok(color);
    }

    // Obtener todos los colores
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Colors.ToList());
    }
}