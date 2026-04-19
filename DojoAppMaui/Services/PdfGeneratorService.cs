using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoAppMaui.Services
{
    public class PdfGeneratorService
    {
        /// <summary>
        /// Genera un reporte de pedidos en formato texto y lo guarda en el almacenamiento local
        /// </summary>
        public async Task<string> GenerateOrdersPdfAsync(List<PedidoDto> pedidos)
        {
            try
            {
                Debug.WriteLine("[PdfGeneratorService] Generando reporte de pedidos");

                if (pedidos == null || pedidos.Count == 0)
                {
                    throw new Exception("No hay pedidos para generar el reporte");
                }

                var textContent = GenerateTextContent(pedidos);

                var fileName = $"Pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                await File.WriteAllTextAsync(filePath, textContent, Encoding.UTF8);

                Debug.WriteLine($"[PdfGeneratorService] Archivo de reporte generado en: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PdfGeneratorService] Error generando reporte: {ex.Message}");
                Debug.WriteLine($"[PdfGeneratorService] Stack: {ex.StackTrace}");
                throw;
            }
        }

        private string GenerateTextContent(List<PedidoDto> pedidos)
        {
            var totalGeneral = pedidos.Sum(p => p.TotalPrice);
            var fechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var text = new StringBuilder();

            // Encabezado
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine("                    REPORTE DE PEDIDOS".PadRight(63));
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine();

            // Información General
            text.AppendLine($"Fecha de Generación: {fechaGeneracion}");
            text.AppendLine($"Total de Pedidos:    {pedidos.Count}");
            text.AppendLine($"Total General:       €{totalGeneral:F2}");
            text.AppendLine();
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine();

            // Detalle de cada pedido
            foreach (var pedido in pedidos)
            {
                text.AppendLine($"PEDIDO #{pedido.Id}");
                text.AppendLine($"─────────────────────────────────────────────────────────────");
                text.AppendLine($"Usuario:        {pedido.UserName} (ID: {pedido.UserId})");
                text.AppendLine($"Campaña:        {pedido.CampaignId}");
                text.AppendLine($"Estado:         {pedido.Status}");
                text.AppendLine($"Fecha:          {pedido.CreatedAt:dd/MM/yyyy HH:mm:ss}");
                text.AppendLine();

                // Tabla de items
                text.AppendLine("ÍTEMS DEL PEDIDO:");
                var headerRow = $"{"Producto",-30} {"Talla",-8} {"Cantidad",-8} {"P. Unit.",-10} {"Subtotal",-10}";
                text.AppendLine(headerRow);
                text.AppendLine(new string('─', 66));

                foreach (var item in pedido.Items)
                {
                    var productName = item.ProductName ?? "N/A";
                    var size = item.Size ?? "N/A";
                    text.AppendLine($"{productName,-30} {size,-8} {item.Quantity,8} €{item.UnitPrice,9:F2} €{item.TotalPrice,9:F2}");
                }

                text.AppendLine(new string('─', 66));
                text.AppendLine($"{"Total Pedido:",-56} €{pedido.TotalPrice,9:F2}");
                text.AppendLine();
            }

            // Total general
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine($"{"TOTAL GENERAL:",-56} €{totalGeneral,9:F2}");
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine();
            text.AppendLine("Este documento contiene información confidencial");
            text.AppendLine($"Generado: {fechaGeneracion}");

            return text.ToString();
        }
    }
}
