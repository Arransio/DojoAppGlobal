using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class ReporteMenuPage : ContentPage
{
    public ReporteMenuPage()
    {
        InitializeComponent();
    }

    // Genera la vista previa del reporte de pedidos y navega a ella.
    private async void OnReporteClicked(object sender, EventArgs e)
    {
        try
        {
            ReporteButton.IsEnabled = false;
            ReporteButton.Text = "CARGANDO...";

            var orderReportService = ServiceHelper.GetService<OrderReportService>();
            var pedidos = await orderReportService.GetAllPedidosAsync();

            if (pedidos.Count == 0)
            {
                await DisplayAlert("Sin pedidos", "No hay pedidos para mostrar", "OK");
                return;
            }

            var pdfService = new PdfGeneratorService();
            var html = pdfService.GenerateHtmlPreview(pedidos);
            Controls.BottomNavBar.SetRoot(new ReportePreviewPage(html, pedidos));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al cargar el reporte: {ex.Message}", "OK");
        }
        finally
        {
            ReporteButton.IsEnabled = true;
            ReporteButton.Text = "VER REPORTE";
        }
    }

    private void OnPagosClicked(object sender, EventArgs e)
    {
        Controls.BottomNavBar.SetRoot(new PagosPage());
    }
}
