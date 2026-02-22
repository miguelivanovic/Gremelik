using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// ESTOS USINGS SON LOS QUE DAN ERROR SI NO TIENES EL PAQUETE INSTALADO
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel;
using System.Reflection.Metadata;

using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Descomenta esto cuando quieras proteger el reporte
    public class ReportesController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService; // <--- AGREGADO

        public ReportesController(GremelikDbContext context, CurrentTenantService tenantService) // <--- AGREGADO
        {
            _context = context;
            _tenantService = tenantService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("recibo/{pagoId}")]
        public async Task<IActionResult> GenerarReciboPago(Guid pagoId)
        {
            // 1. BUSCAR LA INFORMACIÓN EN LA BD
            var pago = await _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Detalles)
                // OJO: Quizás necesites incluir el nombre del concepto si no lo guardaste en Snapshot
                // .ThenInclude(d => d.CuentaPorCobrar) 
                .FirstOrDefaultAsync(p => p.Id == pagoId);

            if (pago == null) return NotFound("El pago no existe.");

            // 2. DISEÑAR EL PDF
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración de la hoja
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // --- ENCABEZADO ---
                    page.Header().Row(row =>
                    {
                        // Logo y Datos Escuela (Izquierda)
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("COLEGIO GREMELIK").Bold().FontSize(18).FontColor(Colors.Blue.Medium);
                            col.Item().Text("Calle Principal #123");
                            col.Item().Text("Gómez Palacio, Durango");
                        });

                        // Datos del Recibo (Derecha)
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("RECIBO DE PAGO").FontSize(16).Bold();
                            col.Item().Text($"Folio: {pago.Folio:00000}").FontColor(Colors.Red.Medium).FontSize(14);
                            col.Item().Text($"{pago.FechaPago:dd/MM/yyyy HH:mm tt}");
                        });
                    });

                    // --- CONTENIDO ---
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // Sección: Datos del Alumno
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Alumno: ").Bold();
                                t.Span($"{pago.Alumno?.Nombre} {pago.Alumno?.PrimerApellido} {pago.Alumno?.SegundoApellido}");
                            });

                            row.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Matrícula: ").Bold();
                                t.Span(pago.Alumno?.Matricula ?? "N/A");
                            });
                        });

                        col.Item().Height(15); // Espacio en blanco

                        // Sección: Tabla de Conceptos
                        col.Item().Table(table =>
                        {
                            // Definir columnas (Descripción ancha, Importe angosto)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            // Encabezados de Tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloCelda).Text("Concepto / Descripción").Bold();
                                header.Cell().Element(EstiloCelda).AlignRight().Text("Importe").Bold();
                            });

                            // Filas de la Tabla (Iteramos los detalles)
                            foreach (var detalle in pago.Detalles)
                            {
                                table.Cell().Element(EstiloCelda).Text(detalle.ConceptoNombreSnapshot);
                                table.Cell().Element(EstiloCelda).AlignRight().Text($"${detalle.MontoAbonado:N2}");
                            }

                            // Pie de Tabla (Totales)
                            table.Cell().ColumnSpan(2).PaddingTop(10).AlignRight().Text(t =>
                            {
                                t.Span("TOTAL PAGADO: ").Bold();
                                t.Span($"${pago.TotalPagado:N2}").Bold().FontSize(14);
                            });

                            // Método de Pago
                            table.Cell().ColumnSpan(2).AlignRight().Text($"Pagado con: {pago.MetodoPago}");

                            if (!string.IsNullOrEmpty(pago.Comentarios))
                            {
                                table.Cell().ColumnSpan(2).PaddingTop(5).Text($"Nota: {pago.Comentarios}").Italic().FontSize(10);
                            }
                        });
                    });

                    // --- PIE DE PÁGINA ---
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Gracias por su pago - ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            // 3. GENERAR EL ARCHIVO PDF EN BYTES
            var pdfBytes = documento.GeneratePdf();

            // 4. RETORNAR EL ARCHIVO AL NAVEGADOR
            //return File(pdfBytes, "application/pdf", $"Recibo_{pago.Folio}.pdf");

            return File(pdfBytes, "application/pdf");
        }

        [HttpGet("corte-caja")]
        public async Task<IActionResult> GenerarCorteCajaPdf([FromQuery] DateTime fecha, [FromQuery] string? cajero)
        {
            // PROTECCIÓN: Saber de qué escuela es
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            // 1. OBTENER LOS DATOS (Usando el tenantId seguro)
            var query = _context.Pagos
                .Include(p => p.Alumno)
                .Where(p => p.EscuelaId == tenantId && p.Activo && p.FechaPago.Date == fecha.Date);

            // Filtro dinámico: Si mandan un nombre de cajero, filtramos. Si no, salen todos.
            if (!string.IsNullOrEmpty(cajero) && cajero != "TODOS")
            {
                query = query.Where(p => p.Usuario == cajero);
            }

            var pagosDelDia = await query.OrderBy(p => p.FechaPago).ToListAsync();

            if (!pagosDelDia.Any()) return NotFound("No hay pagos registrados para estos criterios.");

            var totalGeneral = pagosDelDia.Sum(p => p.TotalPagado);
            var resumenMetodos = pagosDelDia.GroupBy(p => p.MetodoPago)
                                            .Select(g => new { Metodo = g.Key.ToString(), Total = g.Sum(x => x.TotalPagado) })
                                            .ToList();

            // 2. DISEÑAR EL PDF (Formato Ticket / Reporte)
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("CORTE DE CAJA DIARIO").Bold().FontSize(18).AlignCenter().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Fecha: {fecha:dd/MM/yyyy}").AlignCenter();
                        col.Item().Text($"Cajero: {(string.IsNullOrEmpty(cajero) ? "TODOS LOS CAJEROS" : cajero)}").AlignCenter();
                        col.Item().PaddingBottom(10);
                    });

                    page.Content().Column(col =>
                    {
                        // Resumen Financiero
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).Padding(5).Column(c =>
                            {
                                c.Item().Text("RESUMEN DE INGRESOS").Bold().Underline();
                                foreach (var m in resumenMetodos)
                                {
                                    c.Item().Text($"{m.Metodo}: ${m.Total:N2}");
                                }
                                c.Item().PaddingTop(5).Text($"TOTAL EN CAJA: ${totalGeneral:N2}").Bold().FontSize(12);
                            });
                        });

                        col.Item().Height(15);

                        // Tabla de Detalles
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // Folio
                                columns.RelativeColumn(3); // Alumno
                                columns.RelativeColumn(2); // Metodo
                                columns.RelativeColumn(2); // Cajero
                                columns.RelativeColumn(2); // Total
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloCelda).Text("Folio").Bold();
                                header.Cell().Element(EstiloCelda).Text("Alumno").Bold();
                                header.Cell().Element(EstiloCelda).Text("Método").Bold();
                                header.Cell().Element(EstiloCelda).Text("Cajero").Bold();
                                header.Cell().Element(EstiloCelda).AlignRight().Text("Importe").Bold();
                            });

                            foreach (var p in pagosDelDia)
                            {
                                table.Cell().Element(EstiloCelda).Text($"#{p.Folio:00000}");
                                table.Cell().Element(EstiloCelda).Text($"{p.Alumno?.Nombre} {p.Alumno?.PrimerApellido}");
                                table.Cell().Element(EstiloCelda).Text(p.MetodoPago.ToString());
                                table.Cell().Element(EstiloCelda).Text(p.Usuario);
                                table.Cell().Element(EstiloCelda).AlignRight().Text($"${p.TotalPagado:N2}");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
                });
            });

            return File(documento.GeneratePdf(), "application/pdf");
        }

        // FUNCIÓN AUXILIAR PARA DAR ESTILO A LAS CELDAS DE LA TABLA
        // (Esto evita repetir código de bordes y padding)
        static IContainer EstiloCelda(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5);
        }
    }
}