using DojoAppMaui.Services;
using DojoAppMaui.ViewModels;
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
            });

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
