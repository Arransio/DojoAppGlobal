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

            // Debug: verificar si los colores llegaron
            Debug.WriteLine("[PdfGeneratorService] ===== INICIO GENERACIÓN DE REPORTE =====");
            Debug.WriteLine($"[PdfGeneratorService] Total de pedidos: {pedidos.Count}");
            foreach (var pedido in pedidos)
            {
                Debug.WriteLine($"[PdfGeneratorService] Pedido #{pedido.Id} con {pedido.Items.Count} items");
                foreach (var item in pedido.Items)
                {
                    Debug.WriteLine($"[PdfGeneratorService]   - {item.ProductName} (Talla: {item.Size}, Colores: {item.Colors?.Count ?? 0})");
                    if (item.Colors != null && item.Colors.Count > 0)
                    {
                        foreach (var color in item.Colors)
                        {
                            Debug.WriteLine($"[PdfGeneratorService]     * Color: {color.Name} (Role: {color.Role})");
                        }
                    }
                }
            }

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
                text.AppendLine("Producto                       Talla    Colores              Cantidad   P. Unit.   Subtotal");
                text.AppendLine(new string('─', 90));

                foreach (var item in pedido.Items)
                {
                    var productName = item.ProductName ?? "N/A";
                    var size = item.Size ?? "N/A";
                    var colors = FormatColores(item.Colors);
                    text.AppendLine($"{productName,-30} {size,-8} {colors,-20} {item.Quantity,8} €{item.UnitPrice,9:F2} €{item.TotalPrice,9:F2}");
                }

                text.AppendLine(new string('─', 90));
                text.AppendLine($"{"Total Pedido:",-68} €{pedido.TotalPrice,9:F2}");
                text.AppendLine();
            }

            // Resumen de variantes totales
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine("RESUMEN DE VARIANTES (TOTAL DE TODOS LOS PEDIDOS)");
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine();

            var varianteSummary = GenerateVarianteSummary(pedidos);
            foreach (var line in varianteSummary)
            {
                text.AppendLine(line);
            }

            text.AppendLine();

            // Total general
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine($"{"TOTAL GENERAL:",-56} €{totalGeneral,9:F2}");
            text.AppendLine("═══════════════════════════════════════════════════════════════");
            text.AppendLine();
            text.AppendLine("Este documento contiene información confidencial");
            text.AppendLine($"Generado: {fechaGeneracion}");

            Debug.WriteLine("[PdfGeneratorService] ===== FIN GENERACIÓN DE REPORTE =====");

            return text.ToString();
        }

        private string FormatColores(List<ColorDto> colors)
        {
            if (colors == null || colors.Count == 0)
                return "Sin color";

            var colorParts = colors
                .OrderBy(c => c.Role)
                .Select(c => c.Name)
                .ToList();

            return string.Join("/", colorParts);
        }

        private List<string> GenerateVarianteSummary(List<PedidoDto> pedidos)
        {
            var summary = new List<string>();

            Debug.WriteLine("[PdfGeneratorService] ===== INICIANDO GENERACIÓN DE RESUMEN DE VARIANTES =====");

            // Agrupar todas las variantes por: Producto + Talla + Colores
            var allItems = pedidos.SelectMany(p => p.Items).ToList();
            Debug.WriteLine($"[PdfGeneratorService] Total de items en todos los pedidos: {allItems.Count}");

            var variantGroups = allItems
                .GroupBy(item => new
                {
                    Product = item.ProductName,
                    Size = item.Size,
                    Colors = string.Join("/", (item.Colors ?? new List<ColorDto>())
                        .OrderBy(c => c.Role)
                        .Select(c => c.Name))
                })
                .Select(g => new
                {
                    g.Key.Product,
                    g.Key.Size,
                    g.Key.Colors,
                    Total = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Product)
                .ThenBy(x => x.Size)
                .ThenBy(x => x.Colors)
                .ToList();

            Debug.WriteLine($"[PdfGeneratorService] Variantes agrupadas: {variantGroups.Count}");

            if (!variantGroups.Any())
            {
                summary.Add("No hay variantes registradas");
                Debug.WriteLine("[PdfGeneratorService] ❌ No hay variantes para mostrar");
                return summary;
            }

            foreach (var variant in variantGroups)
            {
                var colorText = string.IsNullOrEmpty(variant.Colors) ? "Sin color" : variant.Colors;
                var line = $"• {variant.Product} (Talla {variant.Size}) {colorText}: {variant.Total}";
                summary.Add(line);
                Debug.WriteLine($"[PdfGeneratorService] {line}");
            }

            Debug.WriteLine("[PdfGeneratorService] ===== FIN GENERACIÓN DE RESUMEN DE VARIANTES =====");

            return summary;
        }
    }
}
