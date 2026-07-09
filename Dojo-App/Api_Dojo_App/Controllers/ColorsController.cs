using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Authorization;
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

    // Crear color (solo admin)
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult Create(CreateColorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "El nombre es requerido" });

        var color = new Color { Name = request.Name.Trim() };

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