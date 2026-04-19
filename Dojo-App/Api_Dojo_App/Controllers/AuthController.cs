namespace Api_Dojo_App.Controllers;

using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api_Dojo_App.Data;
using Api_Dojo_App.Models;
using Api_Dojo_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IConfiguration _config;
	private readonly AppDbContext _context;
	private readonly IEmailService _emailService;

	public AuthController(IConfiguration config, AppDbContext context, IEmailService emailService)
	{
		_config = config;
		_context = context;
		_emailService = emailService;
	}

	[HttpPost("login")]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		// Validar entrada
		if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
			return BadRequest("Usuario y contraseña son requeridos");

		// Buscar usuario en la base de datos
		var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

		if (user == null)
			return Unauthorized("Usuario o contraseña incorrectos");

		// Verificar si el email ha sido confirmado
		if (!user.IsEmailConfirmed)
			return Unauthorized("Por favor confirma tu email antes de iniciar sesión. Revisa tu bandeja de entrada.");

		// Verificar contraseña (usar BCrypt o similar en producción)
		if (!VerifyPasswordHash(request.Password, user.PasswordHash))
			return Unauthorized("Usuario o contraseña incorrectos");

		// Login exitoso
		var token = GenerateJwtToken(user.Username, user.Role);

		return Ok(new LoginResponse
		{
			Token = token,
			UserId = user.Id,
			Role = user.Role
		});
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		try
		{
			// Validar entrada
			if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
				return BadRequest("Usuario y contraseña son requeridos");

			if (string.IsNullOrWhiteSpace(request.Email))
				return BadRequest("El email es requerido");

			if (request.Username.Length < 3)
				return BadRequest("Usuario debe tener mínimo 3 caracteres");

			if (request.Password.Length < 4)
				return BadRequest("Contraseña debe tener mínimo 4 caracteres");

			// Validar formato de email
			if (!IsValidEmail(request.Email))
				return BadRequest("El email no es válido");

			// Verificar si el usuario EXISTE
			var existingUser = _context.Users.FirstOrDefault(u => u.Username == request.Username);

			if (existingUser != null)
			{
				Debug.WriteLine($"[AuthController] Usuario '{request.Username}' ya existe en la BD");
				return BadRequest("El usuario ya existe. Por favor elige otro nombre de usuario");
			}

			// Verificar si el email ya existe
			var existingEmail = _context.Users.FirstOrDefault(u => u.Email == request.Email);

			if (existingEmail != null)
			{
				Debug.WriteLine($"[AuthController] Email '{request.Email}' ya existe en la BD");
				return BadRequest("El email ya está registrado");
			}

			// Generar token de confirmación
			var confirmationToken = GenerateConfirmationToken();
			var tokenExpiry = DateTime.UtcNow.AddHours(24);

			// Usuario NO existe, crearlo sin confirmar
			Debug.WriteLine($"[AuthController] Creando nuevo usuario: {request.Username}");

			var newUser = new User
			{
				Username = request.Username,
				PasswordHash = HashPassword(request.Password),
				Email = request.Email,
				Role = "user",
				IsEmailConfirmed = false,
				EmailConfirmationToken = confirmationToken,
				EmailConfirmationTokenExpiry = tokenExpiry
			};

			// Agregar a la base de datos
			_context.Users.Add(newUser);
			await _context.SaveChangesAsync();

			Debug.WriteLine($"[AuthController] Usuario '{request.Username}' creado exitosamente con ID: {newUser.Id}");

				// Generar link de confirmación
				var confirmationLink = $"{_config["AppSettings:FrontendUrl"]}/api/auth/confirm-email?token={confirmationToken}&email={request.Email}";

				// Enviar email de confirmación
				var emailSent = await _emailService.SendConfirmationEmailAsync(request.Email, confirmationLink);

			if (!emailSent)
			{
				Debug.WriteLine($"[AuthController] Error: No se pudo enviar el email de confirmación a {request.Email}");
				return StatusCode(500, "Usuario registrado pero no se pudo enviar el email de confirmación. Por favor intenta más tarde.");
			}

			return Ok(new
			{
				Message = "Usuario registrado exitosamente. Por favor, confirma tu email.",
				UserId = newUser.Id
			});
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[AuthController] Error en register: {ex.Message}");
			return StatusCode(500, $"Error al registrarse: {ex.Message}");
		}
	}

	[Authorize]
	[HttpGet("profile")]
	public IActionResult GetProfile()
	{

		var authHeader = Request.Headers["Authorization"].ToString();

		Debug.WriteLine("RAW HEADER: [" + authHeader + "]");
		Debug.WriteLine("LENGTH HEADER: " + authHeader.Length);
		Debug.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
		Debug.WriteLine($"User.IsInRole('admin'): {User.IsInRole("admin")}");

		var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "No role found";
		Debug.WriteLine($"Role from claims: {role}");

		return Ok(new
		{
			Message = "Acceso autorizado",
			User = User.Identity.Name,
			Role = role,
			IsAdmin = User.IsInRole("admin"),
			AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
		});
	}

	[HttpGet("confirm-email")]
	public async Task<IActionResult> ConfirmEmailLink(string token, string email)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
				return BadRequest("Token y email son requeridos");

			var user = _context.Users.FirstOrDefault(u => u.Email == email);

			if (user == null)
			{
				return Content(GetErrorHtml("Usuario no encontrado"), "text/html");
			}

			if (user.IsEmailConfirmed)
			{
				return Content(GetSuccessHtml("El email ya ha sido confirmado anteriormente"), "text/html");
			}

			if (user.EmailConfirmationToken != token)
			{
				Debug.WriteLine($"[AuthController] Token incorrecto para {email}");
				return Content(GetErrorHtml("Token inválido"), "text/html");
			}

			if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
			{
				Debug.WriteLine($"[AuthController] Token expirado para {email}");
				return Content(GetErrorHtml("El token ha expirado. Por favor, solicita un nuevo email de confirmación"), "text/html");
			}

			// Confirmar email
			user.IsEmailConfirmed = true;
			user.EmailConfirmationToken = null;
			user.EmailConfirmationTokenExpiry = null;

			await _context.SaveChangesAsync();

			Debug.WriteLine($"[AuthController] Email confirmado para usuario: {user.Username}");

			return Content(GetSuccessHtml("¡Email confirmado exitosamente! Ya puedes iniciar sesión en la aplicación"), "text/html");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[AuthController] Error en confirm-email: {ex.Message}");
			return Content(GetErrorHtml($"Error al confirmar email: {ex.Message}"), "text/html");
		}
	}

	[HttpPost("confirm-email")]
	public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Email))
				return BadRequest("Token y email son requeridos");

			var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

			if (user == null)
				return BadRequest("Usuario no encontrado");

			if (user.IsEmailConfirmed)
				return BadRequest("El email ya ha sido confirmado");

			if (user.EmailConfirmationToken != request.Token)
			{
				Debug.WriteLine($"[AuthController] Token incorrecto para {request.Email}");
				return BadRequest("Token inválido");
			}

			if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
			{
				Debug.WriteLine($"[AuthController] Token expirado para {request.Email}");
				return BadRequest("El token ha expirado. Por favor, solicita un nuevo email de confirmación");
			}

			// Confirmar email
			user.IsEmailConfirmed = true;
			user.EmailConfirmationToken = null;
			user.EmailConfirmationTokenExpiry = null;

			await _context.SaveChangesAsync();

			Debug.WriteLine($"[AuthController] Email confirmado para usuario: {user.Username}");

			return Ok(new
			{
				Message = "Email confirmado exitosamente. Ya puedes iniciar sesión."
			});
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[AuthController] Error en confirm-email: {ex.Message}");
			return StatusCode(500, $"Error al confirmar email: {ex.Message}");
		}
	}

	private string GenerateJwtToken(string username, string role = "user")
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
			new Claim(ClaimTypes.Name, username),
			new Claim(ClaimTypes.Role, role)
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

	// Hash de contraseña usando BCrypt
	private string HashPassword(string password)
	{
		return BCrypt.Net.BCrypt.HashPassword(password);
	}

	// Verificar contraseña contra el hash almacenado
	private bool VerifyPasswordHash(string password, string hash)
	{
		return BCrypt.Net.BCrypt.Verify(password, hash);
	}

	// Generar token único de confirmación
	private string GenerateConfirmationToken()
	{
		using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
		{
			byte[] tokenData = new byte[32];
			rng.GetBytes(tokenData);
			return Convert.ToBase64String(tokenData).Replace("/", "_").Replace("+", "-");
		}
	}

	// Validar formato de email
	private bool IsValidEmail(string email)
	{
		try
		{
			var addr = new System.Net.Mail.MailAddress(email);
			return addr.Address == email;
		}
		catch
		{
			return false;
		}
	}

	// HTML helper para éxito
	private string GetSuccessHtml(string message)
	{
		return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
	<meta charset='UTF-8'>
	<meta name='viewport' content='width=device-width, initial-scale=1.0'>
	<title>Email Confirmado</title>
	<style>
		body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif; margin: 0; padding: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); display: flex; justify-content: center; align-items: center; min-height: 100vh; }}
		.container {{ background: white; border-radius: 12px; padding: 40px; text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.3); max-width: 400px; }}
		.success-icon {{ font-size: 60px; margin-bottom: 20px; }}
		h1 {{ color: #2d3748; margin: 0 0 10px 0; font-size: 24px; }}
		p {{ color: #4a5568; line-height: 1.6; margin: 15px 0; }}
		.button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; border-radius: 6px; text-decoration: none; margin-top: 20px; font-weight: 600; }}
		.button:hover {{ background: #764ba2; }}
	</style>
</head>
<body>
	<div class='container'>
		<div class='success-icon'>✅</div>
		<h1>¡Éxito!</h1>
		<p>{message}</p>
		<p style='font-size: 14px; color: #718096; margin-top: 30px;'>Puedes cerrar esta ventana o volver a la aplicación.</p>
	</div>
</body>
</html>";
	}

	// HTML helper para error
	private string GetErrorHtml(string message)
	{
		return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
	<meta charset='UTF-8'>
	<meta name='viewport' content='width=device-width, initial-scale=1.0'>
	<title>Error</title>
	<style>
		body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif; margin: 0; padding: 20px; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); display: flex; justify-content: center; align-items: center; min-height: 100vh; }}
		.container {{ background: white; border-radius: 12px; padding: 40px; text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.3); max-width: 400px; }}
		.error-icon {{ font-size: 60px; margin-bottom: 20px; }}
		h1 {{ color: #2d3748; margin: 0 0 10px 0; font-size: 24px; }}
		p {{ color: #4a5568; line-height: 1.6; margin: 15px 0; }}
		.button {{ display: inline-block; background: #f5576c; color: white; padding: 12px 30px; border-radius: 6px; text-decoration: none; margin-top: 20px; font-weight: 600; }}
		.button:hover {{ background: #f093fb; }}
	</style>
</head>
<body>
	<div class='container'>
		<div class='error-icon'>❌</div>
		<h1>Error</h1>
		<p>{message}</p>
		<p style='font-size: 14px; color: #718096; margin-top: 30px;'>Por favor, intenta nuevamente o contacta con soporte.</p>
	</div>
</body>
</html>";
	}
}