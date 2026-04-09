namespace Api_Dojo_App.Controllers;

using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api_Dojo_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IConfiguration _config;

	public AuthController(IConfiguration config)
	{
		_config = config;
	}

	[HttpPost("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		// ⚠️login fake para probar
		if (request.Username != "admin" || request.Password != "1234")
			return Unauthorized("Usuario o contraseña incorrectos, intentelo de nuevo");

		var token = GenerateJwtToken(request.Username);

		return Ok(new LoginResponse
		{
			Token = token,
			UserId = 1
		});
	}

	[Authorize]
	[HttpGet("profile")]
	public IActionResult GetProfile()
	{
		
		var authHeader = Request.Headers["Authorization"].ToString();

		Debug.WriteLine("RAW HEADER: [" + authHeader + "]");
		Debug.WriteLine("LENGTH HEADER: " + authHeader.Length);

		return Ok(new
		{
			Message = "Acceso autorizado",
			User = User.Identity.Name
		});
	}

	private string GenerateJwtToken(string username)
	{
		var jwtSettings = _config.GetSection("Jwt");

		Debug.WriteLine("KEY GENERATE: " + jwtSettings["Key"]);
		Debug.WriteLine("ISSUER GENERATE: " + jwtSettings["Issuer"]);
		Debug.WriteLine("AUDIENCE GENERATE: " + jwtSettings["Audience"]);

		var key = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(jwtSettings["Key"]));

		

		var credentials = new SigningCredentials(
			key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.Name, username)
		};

		var token = new JwtSecurityToken(
			issuer: jwtSettings["Issuer"],
			audience: jwtSettings["Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(
				double.Parse(jwtSettings["ExpireMinutes"])),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);

	}
}