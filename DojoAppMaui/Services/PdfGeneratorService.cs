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
        /// Genera un PDF con los datos de los pedidos y lo guarda en el almacenamiento local
        /// </summary>
        public async Task<string> GenerateOrdersPdfAsync(List<PedidoDto> pedidos)
        {
            try
            {
                Debug.WriteLine("[PdfGeneratorService] Generando PDF de pedidos");

                var pdfContent = GeneratePdfContent(pedidos);
                
                // Guardar archivo
                var fileName = $"Pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                // Por ahora, vamos a crear un archivo de texto como placeholder
                // hasta que agreguemos la librería de PDF
                await File.WriteAllTextAsync(filePath, pdfContent);

                Debug.WriteLine($"[PdfGeneratorService] PDF guardado en: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PdfGeneratorService] Error generando PDF: {ex.Message}");
                throw;
            }
        }

        private string GeneratePdfContent(List<PedidoDto> pedidos)
        {
            var sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine("                    REPORTE DE PEDIDOS");
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Total de pedidos: {pedidos.Count}");
            sb.AppendLine($"Total general: {pedidos.Sum(p => p.TotalPrice):F2}€");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();

            foreach (var pedido in pedidos)
            {
                sb.AppendLine($"┌─ PEDIDO #{pedido.Id}");
                sb.AppendLine($"│");
                sb.AppendLine($"│  Usuario: {pedido.UserName} (ID: {pedido.UserId})");
                sb.AppendLine($"│  Campaña: {pedido.CampaignId}");
                sb.AppendLine($"│  Estado: {pedido.Status}");
                sb.AppendLine($"│  Fecha: {pedido.CreatedAt:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"│");
                sb.AppendLine($"│  Items:");
                
                foreach (var item in pedido.Items)
                {
                    sb.AppendLine($"│    • {item.ProductName}");
                    sb.AppendLine($"│      Talla: {item.Size} | Cantidad: {item.Quantity} | Precio unitario: {item.UnitPrice:F2}€");
                    sb.AppendLine($"│      Subtotal: {item.TotalPrice:F2}€");
                }

                sb.AppendLine($"│");
                sb.AppendLine($"│  TOTAL PEDIDO: {pedido.TotalPrice:F2}€");
                sb.AppendLine($"└" + new string('─', 60));
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine($"TOTAL GENERAL: {pedidos.Sum(p => p.TotalPrice):F2}€");
            sb.AppendLine("═══════════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }
}
