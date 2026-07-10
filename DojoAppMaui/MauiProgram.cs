using DojoAppMaui.Services;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;

namespace DojoAppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        GlobalFontSettings.FontResolver = new MauiFontResolver();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Sora-Variable.ttf", "Sora");
            });

        // --- Red (IHttpClientFactory) ---
        // La factory comparte y recicla los handlers entre clientes: evita el
        // agotamiento de sockets de crear HttpClient sueltos y deja URL base,
        // timeout y AuthHttpHandler configurados en este único punto.
        builder.Services.AddTransient<AuthHttpHandler>();

        // AuthService va SIN AuthHttpHandler: su 401 significa "credenciales
        // incorrectas", no "sesión caducada" (no debe disparar el logout global).
        builder.Services.AddHttpClient<AuthService>(ConfigureApiClient);

        builder.Services.AddHttpClient<ApiService>(ConfigureApiClient)
            .AddHttpMessageHandler<AuthHttpHandler>();
        builder.Services.AddHttpClient<ProductService>(ConfigureApiClient)
            .AddHttpMessageHandler<AuthHttpHandler>();
        builder.Services.AddHttpClient<PedidosService>(ConfigureApiClient)
            .AddHttpMessageHandler<AuthHttpHandler>();
        builder.Services.AddHttpClient<OrderReportService>(ConfigureApiClient)
            .AddHttpMessageHandler<AuthHttpHandler>();

        // --- ViewModels ---
        // Transient: cada pantalla recibe su propia instancia con estado limpio.
        builder.Services.AddTransient<ViewModels.HistorialPedidosViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureApiClient(HttpClient client)
    {
        client.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
        client.Timeout = AppConfig.HttpTimeout;
    }
}
