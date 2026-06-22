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

    private async void OnCerrarClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnDescargarClicked(object sender, EventArgs e)
    {
        try
        {
            DescargarButton.IsEnabled = false;
            DescargarButton.Text = "...";

            var pdfService = new PdfGeneratorService();
            var filePath = await pdfService.GenerateOrdersPdfAsync(_pedidos);

            DescargarButton.IsEnabled = true;
            DescargarButton.Text = "💾 PDF";

            var action = await DisplayActionSheet(
                $"PDF generado: {Path.GetFileName(filePath)}",
                "Cerrar", null, "Compartir");

            if (action == "Compartir")
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Reporte de pedidos",
                    File = new ShareFile(filePath)
                });
            }
        }
        catch (Exception ex)
        {
            DescargarButton.IsEnabled = true;
            DescargarButton.Text = "💾 PDF";
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
