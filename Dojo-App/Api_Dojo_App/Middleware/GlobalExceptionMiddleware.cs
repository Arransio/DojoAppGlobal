using System.Text.Json;

namespace Api_Dojo_App.Middleware
{
    // Captura cualquier excepción no controlada, la registra con ILogger y
    // devuelve al cliente un 500 genérico sin detalles internos (nada de
    // stack traces, inner exceptions ni datos de la BD).
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción no controlada en {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var payload = JsonSerializer.Serialize(new { error = "Error interno del servidor" });
                    await context.Response.WriteAsync(payload);
                }
            }
        }
    }
}
