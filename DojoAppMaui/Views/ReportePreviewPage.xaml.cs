using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class ReportePreviewPage : ContentPage
{
    private readonly List<PedidoDto> _pedidos;

    public ReportePreviewPage(string htmlContent, List<PedidoDto> pedidos)
    {
        InitializeComponent();
        _pedidos = pedidos;
        ReporteWebView.Source = new HtmlWebViewSource { Html = htmlContent };
    }

    private void OnCerrarClicked(object sender, EventArgs e)
    {
        // Volvemos al menú de Reporte, desde donde se abrió la vista previa.
        Controls.BottomNavBar.SetRoot(new ReporteMenuPage());
    }

    // Enviar: comparte el PDF por el sistema (email, mensajería, etc.).
    private async void OnEnviarClicked(object sender, EventArgs e)
    {
        var filePath = await GeneratePdfAsync(EnviarButton, "📤 ENVIAR");
        if (filePath == null)
            return;

        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Reporte de pedidos",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo compartir: {ex.Message}", "OK");
        }
    }

    // Guardar en dispositivo: exporta el PDF a una carpeta accesible (Descargas en Android).
    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var filePath = await GeneratePdfAsync(GuardarButton, "💾 GUARDAR");
        if (filePath == null)
            return;

        try
        {
            var fileName = Path.GetFileName(filePath);
#if ANDROID
            var saved = SaveToDownloadsAndroid(filePath, fileName);
            if (saved)
                await DisplayAlert("Guardado", $"El reporte se guardó en Descargas:\n{fileName}", "OK");
            else
                await FallbackShareAsync(filePath);
#else
            // En el resto de plataformas usamos la hoja de compartir (opción "Guardar en archivos").
            await FallbackShareAsync(filePath);
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }

    // Genera el PDF gestionando el estado visual del botón. Devuelve la ruta o null si falla.
    private async Task<string?> GeneratePdfAsync(Button button, string restoreText)
    {
        try
        {
            button.IsEnabled = false;
            button.Text = "...";

            var pdfService = new PdfGeneratorService();
            var filePath = await pdfService.GenerateOrdersPdfAsync(_pedidos);

            button.IsEnabled = true;
            button.Text = restoreText;
            return filePath;
        }
        catch (Exception ex)
        {
            button.IsEnabled = true;
            button.Text = restoreText;
            await DisplayAlert("Error", $"Error al generar el PDF: {ex.Message}", "OK");
            return null;
        }
    }

    private async Task FallbackShareAsync(string filePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Guardar reporte",
            File = new ShareFile(filePath)
        });
    }

#if ANDROID
    // Copia el PDF a la carpeta pública de Descargas usando MediaStore (API 29+).
    private static bool SaveToDownloadsAndroid(string sourcePath, string fileName)
    {
        try
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(29))
                return false; // En versiones anteriores recurrimos a la hoja de compartir.

            var context = Android.App.Application.Context;
            var resolver = context.ContentResolver;
            if (resolver == null)
                return false;

            var values = new Android.Content.ContentValues();
            values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
            values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "application/pdf");
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

            var uri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, values);
            if (uri == null)
                return false;

            using var os = resolver.OpenOutputStream(uri);
            if (os == null)
                return false;

            var bytes = System.IO.File.ReadAllBytes(sourcePath);
            os.Write(bytes, 0, bytes.Length);
            os.Flush();
            return true;
        }
        catch
        {
            return false;
        }
    }
#endif
}
