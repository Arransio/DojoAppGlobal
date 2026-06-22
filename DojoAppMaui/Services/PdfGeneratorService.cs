using System.Diagnostics;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace DojoAppMaui.Services
{
    public class PdfGeneratorService
    {
        // Dimensiones A4 en puntos
        private const double MarginLeft = 42.5;
        private const double MarginRight = 42.5;
        private const double MarginTop = 42.5;
        private const double MarginBottom = 42.5;
        private const double PageWidth = 595.28;
        private const double PageHeight = 841.89;
        private const double ContentWidth = PageWidth - MarginLeft - MarginRight;

        // Anchos de columnas para tabla de ítems
        private static readonly double[] ColWidths = {
            ContentWidth * 0.30, // Producto
            ContentWidth * 0.10, // Talla
            ContentWidth * 0.20, // Colores
            ContentWidth * 0.10, // Cantidad
            ContentWidth * 0.15, // P.Unit.
            ContentWidth * 0.15  // Subtotal
        };

        private static readonly XColor Orange = XColor.FromArgb(255, 111, 0);
        private static readonly XColor OrangeLight = XColor.FromArgb(255, 243, 224);
        private static readonly XColor RowAlt = XColor.FromArgb(250, 250, 250);
        private static readonly XColor RowBorder = XColor.FromArgb(238, 238, 238);
        private static readonly XColor Grey = XColor.FromArgb(158, 158, 158);

        // Estado del renderizado
        private PdfDocument _doc = null!;
        private PdfPage _page = null!;
        private XGraphics _gfx = null!;
        private double _y;
        private XFont _regular = null!;
        private XFont _bold = null!;
        private XFont _small = null!;
        private XFont _smallBold = null!;
        private XFont _large = null!;
        private XFont _medium = null!;
        private XFont _mediumBold = null!;

        public async Task<string> GenerateOrdersPdfAsync(List<PedidoDto> pedidos)
        {
            try
            {
                Debug.WriteLine("[PdfGeneratorService] Generando reporte PDF");

                if (pedidos == null || pedidos.Count == 0)
                    throw new Exception("No hay pedidos para generar el reporte");

                var fileName = $"Pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                var totalGeneral = (double)pedidos.Sum(p => p.TotalPrice);
                var fechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                await Task.Run(() => Render(pedidos, totalGeneral, fechaGeneracion, filePath));

                Debug.WriteLine($"[PdfGeneratorService] PDF guardado en: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PdfGeneratorService] Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void Render(List<PedidoDto> pedidos, double totalGeneral, string fechaGeneracion, string filePath)
        {
            _doc = new PdfDocument();
            _doc.Info.Title = "Reporte de Pedidos";

            _regular = new XFont("OpenSans", 10);
            _bold = new XFont("OpenSans", 10, XFontStyle.Bold);
            _small = new XFont("OpenSans", 9);
            _smallBold = new XFont("OpenSans", 9, XFontStyle.Bold);
            _large = new XFont("OpenSans", 20, XFontStyle.Bold);
            _medium = new XFont("OpenSans", 12);
            _mediumBold = new XFont("OpenSans", 12, XFontStyle.Bold);

            NewPage();

            DrawHeader(fechaGeneracion, pedidos.Count, totalGeneral);
            foreach (var pedido in pedidos)
                DrawPedido(pedido);
            DrawVarianteSummary(pedidos);
            DrawTotalGeneral(totalGeneral);
            DrawFooter(fechaGeneracion);

            _gfx.Dispose();
            _doc.Save(filePath);
        }

        // ── Gestión de páginas ──────────────────────────────────────────────

        private void NewPage()
        {
            _gfx?.Dispose();
            _page = _doc.AddPage();
            _page.Size = PdfSharpCore.PageSize.A4;
            _gfx = XGraphics.FromPdfPage(_page);
            _y = MarginTop;
        }

        private void EnsureSpace(double needed)
        {
            if (_y + needed > PageHeight - MarginBottom)
                NewPage();
        }

        // ── Primitivas de dibujo ────────────────────────────────────────────

        private void FillRect(double y, double height, XColor color)
        {
            _gfx.DrawRectangle(new XSolidBrush(color), MarginLeft, y, ContentWidth, height);
        }

        private void DrawText(string text, XFont font, XColor color, double x, double y,
            double width, double height, XStringAlignment hAlign = XStringAlignment.Near)
        {
            var fmt = new XStringFormat
            {
                Alignment = hAlign,
                LineAlignment = XLineAlignment.Center
            };
            _gfx.DrawString(text, font, new XSolidBrush(color),
                new XRect(x, y, width, height), fmt);
        }

        private void DrawHLine(double y, XColor color, double thickness = 1)
        {
            _gfx.DrawLine(new XPen(color, thickness), MarginLeft, y, MarginLeft + ContentWidth, y);
        }

        // ── Secciones del documento ─────────────────────────────────────────

        private void DrawHeader(string fecha, int totalPedidos, double totalGeneral)
        {
            const double h = 36;
            EnsureSpace(h + 70);

            FillRect(_y, h, Orange);
            DrawText("REPORTE DE PEDIDOS", _large, XColors.White, MarginLeft, _y, ContentWidth, h, XStringAlignment.Center);
            _y += h + 10;

            DrawText($"Fecha de Generación: {fecha}", _regular, XColors.Black, MarginLeft, _y, ContentWidth, 16);
            _y += 16;
            DrawText($"Total de Pedidos: {totalPedidos}", _regular, XColors.Black, MarginLeft, _y, ContentWidth, 16);
            _y += 16;
            DrawText($"Total General: €{totalGeneral:F2}", _bold, XColors.Black, MarginLeft, _y, ContentWidth, 16);
            _y += 22;

            DrawHLine(_y, Orange, 2);
            _y += 12;
        }

        private void DrawPedido(PedidoDto pedido)
        {
            EnsureSpace(110);
            _y += 6;

            const double titleH = 24;
            FillRect(_y, titleH, OrangeLight);
            DrawText($"PEDIDO #{pedido.Id}", _mediumBold, XColors.Black, MarginLeft + 4, _y, ContentWidth - 8, titleH);
            _y += titleH + 4;

            string[] info = {
                $"Usuario: {pedido.UserName} (ID: {pedido.UserId})",
                $"Campaña: {pedido.CampaignId}",
                $"Estado: {pedido.Status}",
                $"Fecha: {pedido.CreatedAt:dd/MM/yyyy HH:mm:ss}"
            };
            foreach (var line in info)
            {
                DrawText(line, _regular, XColors.Black, MarginLeft + 8, _y, ContentWidth - 8, 14);
                _y += 14;
            }
            _y += 6;

            DrawItemsTable(pedido);

            DrawText($"Total Pedido: €{pedido.TotalPrice:F2}", _bold, XColors.Black,
                MarginLeft, _y, ContentWidth - 2, 18, XStringAlignment.Far);
            _y += 22;
        }

        private void DrawItemsTable(PedidoDto pedido)
        {
            const double rowH = 16;

            EnsureSpace(rowH * 2);
            FillRect(_y, rowH, Orange);
            DrawTableRow(new[] { "Producto", "Talla", "Colores", "Cantidad", "P.Unit.", "Subtotal" },
                _smallBold, XColors.White, _y, rowH);
            _y += rowH;

            bool isEven = false;
            foreach (var item in pedido.Items)
            {
                EnsureSpace(rowH);
                if (isEven) FillRect(_y, rowH, RowAlt);
                isEven = !isEven;
                DrawHLine(_y + rowH, RowBorder, 0.5);

                DrawTableRow(new[] {
                    item.ProductName ?? "N/A",
                    item.Size ?? "N/A",
                    FormatColores(item.Colors),
                    item.Quantity.ToString(),
                    $"€{item.UnitPrice:F2}",
                    $"€{item.TotalPrice:F2}"
                }, _small, XColors.Black, _y, rowH);
                _y += rowH;
            }
            _y += 6;
        }

        private void DrawTableRow(string[] values, XFont font, XColor color, double y, double rowH)
        {
            double x = MarginLeft + 3;
            for (int i = 0; i < values.Length && i < ColWidths.Length; i++)
            {
                var align = i >= 3 ? XStringAlignment.Far : XStringAlignment.Near;
                DrawText(values[i], font, color, x, y, ColWidths[i] - 4, rowH, align);
                x += ColWidths[i];
            }
        }

        private void DrawVarianteSummary(List<PedidoDto> pedidos)
        {
            EnsureSpace(70);
            _y += 10;

            DrawHLine(_y, Orange, 2);
            _y += 10;

            const double titleH = 28;
            FillRect(_y, titleH, Orange);
            DrawText("RESUMEN DE VARIANTES (TOTAL DE TODOS LOS PEDIDOS)",
                _bold, XColors.White, MarginLeft + 4, _y, ContentWidth - 8, titleH);
            _y += titleH + 10;

            foreach (var line in GenerateVarianteSummary(pedidos))
            {
                EnsureSpace(14);
                DrawText(line, _small, XColors.Black, MarginLeft, _y, ContentWidth, 14);
                _y += 14;
            }
            _y += 10;
        }

        private void DrawTotalGeneral(double totalGeneral)
        {
            EnsureSpace(36);
            const double h = 32;
            FillRect(_y, h, Orange);
            DrawText("TOTAL GENERAL:", _mediumBold, XColors.White, MarginLeft + 6, _y, ContentWidth * 0.65, h);
            DrawText($"€{totalGeneral:F2}", _mediumBold, XColors.White,
                MarginLeft + ContentWidth * 0.65, _y, ContentWidth * 0.35, h, XStringAlignment.Far);
            _y += h + 20;
        }

        private void DrawFooter(string fechaGeneracion)
        {
            EnsureSpace(32);
            DrawText("Este documento contiene información confidencial",
                _small, Grey, MarginLeft, _y, ContentWidth, 14, XStringAlignment.Center);
            _y += 14;
            DrawText($"Generado: {fechaGeneracion}",
                _small, Grey, MarginLeft, _y, ContentWidth, 14, XStringAlignment.Center);
        }

        // ── Previsualización HTML ────────────────────────────────────────────

        public string GenerateHtmlPreview(List<PedidoDto> pedidos)
        {
            var totalGeneral = (double)pedidos.Sum(p => p.TotalPrice);
            var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var sb = new System.Text.StringBuilder();

            sb.Append(@"<!DOCTYPE html><html><head><meta charset='UTF-8'>
<meta name='viewport' content='width=device-width,initial-scale=1.0'>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:sans-serif;font-size:14px;color:#333;background:#f5f5f5}
.header{background:#FF6F00;color:#fff;padding:18px;text-align:center;font-size:20px;font-weight:bold}
.info-block{background:#fff;padding:14px 16px;border-bottom:2px solid #FF6F00}
.info-block p{margin:3px 0;font-size:13px}
.info-block .total{font-weight:bold;font-size:15px}
.pedido{background:#fff;margin:10px 8px;border-radius:8px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,.15)}
.pedido-title{background:#FFF3E0;padding:10px 12px;font-weight:bold;font-size:15px;border-left:4px solid #FF6F00}
.pedido-info{padding:8px 12px;font-size:12px;color:#555}
.pedido-info p{margin:2px 0}
table{width:100%;border-collapse:collapse;margin-top:4px}
th{background:#FF6F00;color:#fff;padding:7px 6px;font-size:11px;text-align:left}
th.r{text-align:right}
td{padding:6px;font-size:11px;border-bottom:1px solid #eee}
td.r{text-align:right}
tr:nth-child(even) td{background:#fafafa}
.pedido-total{text-align:right;padding:8px 12px;font-weight:bold;font-size:13px;background:#fff}
.divider{height:3px;background:#FF6F00;margin:16px 8px 8px}
.section-title{background:#FF6F00;color:#fff;padding:12px 16px;font-weight:bold;font-size:15px;margin:0 8px;border-radius:6px 6px 0 0}
.variants{background:#fff;margin:0 8px;padding:10px 12px;border-radius:0 0 6px 6px;box-shadow:0 1px 3px rgba(0,0,0,.1)}
.variant{font-size:12px;padding:2px 0;color:#444}
.total-general{background:#FF6F00;color:#fff;margin:14px 8px;padding:14px 16px;border-radius:8px;display:flex;justify-content:space-between;align-items:center;font-size:16px;font-weight:bold}
.footer{text-align:center;color:#aaa;font-size:11px;padding:20px 8px 30px}
</style></head><body>");

            sb.Append($"<div class='header'>REPORTE DE PEDIDOS</div>");
            sb.Append($"<div class='info-block'><p>Fecha de Generación: {fecha}</p><p>Total de Pedidos: {pedidos.Count}</p><p class='total'>Total General: €{totalGeneral:F2}</p></div>");

            foreach (var pedido in pedidos)
            {
                var pedidoTotal = (double)pedido.TotalPrice;
                sb.Append($"<div class='pedido'>");
                sb.Append($"<div class='pedido-title'>PEDIDO #{pedido.Id}</div>");
                sb.Append($"<div class='pedido-info'><p>Usuario: {Esc(pedido.UserName)} (ID: {pedido.UserId})</p><p>Campaña: {pedido.CampaignId}</p><p>Estado: {Esc(pedido.Status)}</p><p>Fecha: {pedido.CreatedAt:dd/MM/yyyy HH:mm:ss}</p></div>");
                sb.Append("<table><thead><tr><th>Producto</th><th>Talla</th><th>Colores</th><th class='r'>Cantidad</th><th class='r'>P.Unit.</th><th class='r'>Subtotal</th></tr></thead><tbody>");
                foreach (var item in pedido.Items)
                {
                    var unitPrice = (double)item.UnitPrice;
                    var totalPrice = (double)item.TotalPrice;
                    sb.Append($"<tr><td>{Esc(item.ProductName)}</td><td>{Esc(item.Size)}</td><td>{Esc(FormatColores(item.Colors))}</td><td class='r'>{item.Quantity}</td><td class='r'>€{unitPrice:F2}</td><td class='r'>€{totalPrice:F2}</td></tr>");
                }
                sb.Append("</tbody></table>");
                sb.Append($"<div class='pedido-total'>Total Pedido: €{pedidoTotal:F2}</div>");
                sb.Append("</div>");
            }

            sb.Append("<div class='divider'></div>");
            sb.Append("<div class='section-title'>RESUMEN DE VARIANTES</div>");
            sb.Append("<div class='variants'>");
            foreach (var line in GenerateVarianteSummary(pedidos))
                sb.Append($"<p class='variant'>{Esc(line)}</p>");
            sb.Append("</div>");

            sb.Append($"<div class='total-general'><span>TOTAL GENERAL:</span><span>€{totalGeneral:F2}</span></div>");
            sb.Append($"<div class='footer'>Documento confidencial · Generado: {fecha}</div>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private static string Esc(string? s) =>
            System.Net.WebUtility.HtmlEncode(s ?? "N/A");

        // ── Helpers de datos ────────────────────────────────────────────────

        private string FormatColores(List<ColorDto> colors)
        {
            if (colors == null || colors.Count == 0) return "Sin color";
            return string.Join("/", colors.OrderBy(c => c.Role).Select(c => c.Name));
        }

        private List<string> GenerateVarianteSummary(List<PedidoDto> pedidos)
        {
            var allItems = pedidos.SelectMany(p => p.Items).ToList();
            var groups = allItems
                .GroupBy(item => new
                {
                    Product = item.ProductName,
                    Size = item.Size,
                    Colors = string.Join("/", (item.Colors ?? new List<ColorDto>())
                        .OrderBy(c => c.Role).Select(c => c.Name))
                })
                .Select(g => new { g.Key.Product, g.Key.Size, g.Key.Colors, Total = g.Sum(x => x.Quantity) })
                .OrderBy(x => x.Product).ThenBy(x => x.Size).ThenBy(x => x.Colors)
                .ToList();

            if (!groups.Any())
                return new List<string> { "No hay variantes registradas" };

            return groups.Select(v =>
            {
                var c = string.IsNullOrEmpty(v.Colors) ? "Sin color" : v.Colors;
                return $"• {v.Product} (Talla {v.Size}) {c}: {v.Total}";
            }).ToList();
        }
    }
}
