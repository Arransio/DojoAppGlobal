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
            .Include(c => c.Products)
                .ThenInclude(p => p.ProductVariants)
            .FirstOrDefault(c => c.IsActive);

        if (campaign == null)
            return NotFound("No hay campaña activa");

        return Ok(campaign);
    }

    // Crear campaña (solo admin)
    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult CreateCampaign(Campaign campaign)
    {
        _context.Campaigns.Add(campaign);
        _context.SaveChanges();

        return Ok(campaign);
    }
}
