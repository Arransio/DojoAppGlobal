using System.Diagnostics;
using System.Text;
using Api_Dojo_App.Data;
using Api_Dojo_App.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");

Debug.WriteLine("KEY VALIDATE: " + jwtSettings["Key"]);
Debug.WriteLine("ISSUER VALIDATE: " + jwtSettings["Issuer"]);
Debug.WriteLine("AUDIENCE VALIDATE: " + jwtSettings["Audience"]);


//Esquema de autenticacion de peticiones con token
var key = builder.Configuration["Jwt:Key"];



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
		Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

		// Mapear los claims correctamente para que [Authorize(Roles = "admin")] funcione
		NameClaimType = System.Security.Claims.ClaimTypes.Name,
		RoleClaimType = System.Security.Claims.ClaimTypes.Role
	};
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			Debug.WriteLine("AUTH FAILED: " + context.Exception.Message);
			return Task.CompletedTask;
		},
		OnTokenValidated = context =>
		{
			Debug.WriteLine("TOKEN VALIDATED OK");
			var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
			Debug.WriteLine($"Claims: {string.Join(", ", claims ?? new List<string>())}");
			return Task.CompletedTask;
		}
	};
});

builder.Services.AddAuthorization();
// Add services to the container.
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite("Data Source=app.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Servir archivos estáticos (wwwroot)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
