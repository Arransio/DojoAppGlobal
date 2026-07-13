using System.Text;
using System.Threading.RateLimiting;
using Api_Dojo_App.Data;
using Api_Dojo_App.Middleware;
using Api_Dojo_App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// --- Secretos obligatorios (user-secrets en desarrollo, variables de entorno en producción) ---
// Fail-fast: si faltan, la app no debe arrancar con una configuración insegura.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
	throw new InvalidOperationException(
		"Falta 'Jwt:Key' (mínimo 32 caracteres). Configúrala con 'dotnet user-secrets set \"Jwt:Key\" \"...\"' o una variable de entorno.");

if (string.IsNullOrWhiteSpace(builder.Configuration["EmailSettings:SenderPassword"]))
	throw new InvalidOperationException(
		"Falta 'EmailSettings:SenderPassword'. Configúrala con 'dotnet user-secrets set \"EmailSettings:SenderPassword\" \"...\"' o una variable de entorno.");

// URL pública para los links de los emails. Depende del entorno: en desarrollo
// viene de appsettings.Development.json (emulador); en producción DEBE configurarse
// con la URL real o los correos de confirmación llevarían enlaces rotos.
if (string.IsNullOrWhiteSpace(builder.Configuration["AppSettings:FrontendUrl"]))
	throw new InvalidOperationException(
		"Falta 'AppSettings:FrontendUrl' (URL base de los links de email para este entorno).");

//Esquema de autenticacion de peticiones con token
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,

		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],

		IssuerSigningKey = new SymmetricSecurityKey(
		Encoding.UTF8.GetBytes(jwtKey)),

		// Mapear los claims correctamente para que [Authorize(Roles = "admin")] funcione
		NameClaimType = System.Security.Claims.ClaimTypes.Name,
		RoleClaimType = System.Security.Claims.ClaimTypes.Role
	};
});

builder.Services.AddAuthorization();

// Rate limiting en endpoints sensibles (login/register/confirm-email):
// ventana fija por IP para frenar fuerza bruta y spam de emails.
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddPolicy("auth", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 10,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			}));
});

// Add services to the container.
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));

var app = builder.Build();

// --- Migraciones + seed de roles al arranque ---
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

	// 1) Aplicar migraciones pendientes automáticamente.
	// Migrate() crea la BD si no existe y aplica solo lo que falte (si no hay nada
	// pendiente, no hace nada). Esto elimina el paso manual 'dotnet ef database update'
	// en cada despliegue y evita el fallo "el API no arranca contra una BD nueva".
	// Un fallo aquí SÍ es fatal: arrancar con un esquema incorrecto daría errores 500
	// silenciosos en runtime; es preferible que el arranque falle de forma ruidosa.
	try
	{
		await db.Database.MigrateAsync();
		logger.LogInformation("Migraciones aplicadas correctamente");
	}
	catch (Exception ex)
	{
		logger.LogCritical(ex, "Fallo al aplicar las migraciones al arranque");
		throw;
	}

	// 2) Seed de roles. El rol vive en la BD (User.Role). Aseguramos que test1 es
	// admin y que los usuarios antiguos sin rol pasan a "user", para no cambiar el
	// comportamiento actual de la app (su gating local de admin usa test1).
	// Un fallo aquí NO es fatal: el API puede servir aunque el seed no se aplique;
	// lo registramos y seguimos.
	try
	{
		var changed = false;

		var admin = await db.Users.FirstOrDefaultAsync(u => u.Username == "test1");
		if (admin != null && admin.Role != "admin")
		{
			admin.Role = "admin";
			changed = true;
		}

		await foreach (var user in db.Users
			.Where(u => string.IsNullOrEmpty(u.Role) && u.Username != "test1")
			.AsAsyncEnumerable())
		{
			user.Role = "user";
			changed = true;
		}

		if (changed)
		{
			await db.SaveChangesAsync();
			logger.LogInformation("Seed de roles aplicado");
		}
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Fallo al aplicar el seed de roles al arranque (el API continúa)");
	}
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	// En desarrollo la app MAUI usa http://10.0.2.2:5221 (emulador Android),
	// por eso solo forzamos HTTPS fuera de Development.
	app.UseHttpsRedirection();
}

// Errores no controlados -> 500 genérico + log (sin stack traces al cliente)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Servir archivos estáticos (wwwroot)
app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
