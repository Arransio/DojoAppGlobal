using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CampaignsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("active")]
    public IActionResult GetActiveCampaign()
    {
        var campaign = _context.Campaigns
            .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductVariants)
            .FirstOrDefault(c => c.IsActive);

        if (campaign == null)
            return NotFound("No hay campaña activa");

        return Ok(campaign);
    }

    // Crear campaña (solo admin)
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult CreateCampaign(CreateCampaignRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "El nombre es requerido" });

        if (request.EndDate <= request.StartDate)
            return BadRequest(new { error = "La fecha de fin debe ser posterior a la de inicio" });

        var campaign = new Campaign
        {
            Name = request.Name.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        _context.Campaigns.Add(campaign);
        _context.SaveChanges();

        return Ok(campaign);
    }
}
