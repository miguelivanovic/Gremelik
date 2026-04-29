using Gremelik.core.Entities;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] 
    public class ReportesController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public ReportesController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpGet("recibo/{pagoId}")]
        public async Task<IActionResult> GenerarReciboPago(Guid pagoId)
        {
            var pago = await _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == pagoId);

            if (pago == null) return NotFound("El pago no existe.");

            var escuela = await _context.Escuelas.FindAsync(pago.EscuelaId);

            // BUSCAR SI ESTE PAGO TIENE UN CFDI TIMBRADO ASOCIADO
            var factura = await _context.Facturas
                .Include(f => f.Tutor)
                .FirstOrDefaultAsync(f => f.PagoId == pagoId && !string.IsNullOrEmpty(f.Uuid));

            bool esFactura = factura != null;

            byte[]? logoBytes = null;
            if (!string.IsNullOrEmpty(escuela?.LogoUrl))
            {
                try
                {
                    using var httpClient = new HttpClient();
                    logoBytes = await httpClient.GetByteArrayAsync(escuela.LogoUrl);
                }
                catch { }
            }

            // GENERAR EL QR EN MEMORIA (SI ES FACTURA)
            byte[]? qrBytes = null;
            if (esFactura)
            {
                try
                {
                    string rfcEmisor = "ESCUELA123";
                    string rfcReceptor = factura!.Tutor?.RFC ?? "XAXX010101000";
                    string urlSat = $"https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id={factura.Uuid}&re={rfcEmisor}&rr={rfcReceptor}&tt={pago.TotalPagado}&fe=SIMUL";

                    using var qrGenerator = new QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(urlSat, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    qrBytes = qrCode.GetGraphic(5);
                }
                catch { }
            }

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Row(row =>
                    {
                        if (logoBytes != null)
                        {
                            row.ConstantItem(60).Image(logoBytes);
                            row.ConstantItem(15);
                        }

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(escuela?.Nombre ?? "COLEGIO").Bold().FontSize(18).FontColor(Colors.Blue.Medium);

                            if (esFactura)
                            {
                                col.Item().Text($"RFC: ESCUELA123").FontSize(9);
                                col.Item().Text($"Régimen Fiscal: 601 - General de Ley Personas Morales").FontSize(9);
                            }
                            else if (!string.IsNullOrEmpty(escuela?.Slogan))
                            {
                                col.Item().Text(escuela.Slogan).Italic().FontSize(10).FontColor(Colors.Grey.Darken2);
                            }
                            else
                            {
                                col.Item().Text("Comprobante de Ingreso").FontSize(10).FontColor(Colors.Grey.Medium);
                            }
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(esFactura ? "FACTURA ELECTRÓNICA" : "RECIBO DE PAGO")
                                      .FontSize(14).Bold().FontColor(esFactura ? Colors.Blue.Darken2 : Colors.Black);

                            col.Item().Text($"Folio: {pago.Folio:00000}").FontColor(Colors.Red.Medium).FontSize(14).Bold();
                            col.Item().Text($"{pago.FechaPago:dd/MM/yyyy HH:mm tt}");

                            if (esFactura)
                            {
                                col.Item().PaddingTop(5).Text("Folio Fiscal (UUID):").FontSize(8).Bold();
                                col.Item().Text(factura!.Uuid).FontSize(8);
                            }
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Row(row =>
                        {
                            if (esFactura)
                            {
                                row.RelativeItem().Column(c => {
                                    c.Item().Text("Facturado a:").Bold().FontSize(10);
                                    c.Item().Text($"{factura!.Tutor?.Nombre} {factura.Tutor?.PrimerApellido}");
                                    c.Item().Text($"RFC: {factura.Tutor?.RFC}").FontSize(9);
                                });
                                row.RelativeItem().AlignRight().Column(c => {
                                    c.Item().Text("Alumno:").Bold().FontSize(10).AlignRight();
                                    c.Item().Text($"{pago.Alumno?.Nombre} {pago.Alumno?.PrimerApellido}").AlignRight();
                                    c.Item().Text($"Matrícula: {pago.Alumno?.Matricula}").FontSize(9).AlignRight();
                                });
                            }
                            else
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
                            }
                        });

                        col.Item().Height(15);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloCelda).Text("Concepto / Descripción").Bold();
                                header.Cell().Element(EstiloCelda).AlignRight().Text("Importe").Bold();
                            });

                            foreach (var detalle in pago.Detalles.Where(d => d.Activo))
                            {
                                table.Cell().Element(EstiloCelda).Text(detalle.ConceptoNombreSnapshot);
                                table.Cell().Element(EstiloCelda).AlignRight().Text($"${detalle.MontoAbonado:N2}");
                            }

                            table.Cell().ColumnSpan(2).PaddingTop(10).AlignRight().Text(t =>
                            {
                                t.Span("TOTAL PAGADO: ").Bold();
                                t.Span($"${pago.TotalPagado:N2}").Bold().FontSize(14);
                            });

                            table.Cell().ColumnSpan(2).AlignRight().Text($"Pagado con: {pago.MetodoPago}");

                            if (esFactura)
                            {
                                table.Cell().ColumnSpan(2).AlignRight().Text("Método de Pago: PUE (Pago en una sola exhibición)").FontSize(9);
                                table.Cell().ColumnSpan(2).AlignRight().Text("Uso CFDI: D10 - Pagos por servicios educativos (colegiaturas)").FontSize(9);
                            }

                            if (!string.IsNullOrEmpty(pago.Comentarios))
                            {
                                table.Cell().ColumnSpan(2).PaddingTop(5).Text($"Nota: {pago.Comentarios}").Italic().FontSize(10);
                            }
                        });
                    });

                    page.Footer().Column(footer =>
                    {
                        if (esFactura && qrBytes != null)
                        {
                            footer.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(5).Row(row =>
                            {
                                row.ConstantItem(80).Image(qrBytes);

                                row.RelativeItem().PaddingLeft(10).Column(c =>
                                {
                                    c.Item().Text("Sello Digital del CFDI").Bold().FontSize(6);
                                    c.Item().Text("MIGUELDEV/Gremelik/CadenaSimuladaDeSelloDigitalDePruebaParaPintarElPDF/Wq234k...").FontSize(5).FontColor(Colors.Grey.Darken1);

                                    c.Item().PaddingTop(3).Text("Sello Digital del SAT").Bold().FontSize(6);
                                    c.Item().Text("SAT/Verificacion/OtraCadenaSuperLargaQueTeRegresaElPACAlTimbrar/Lp987m...").FontSize(5).FontColor(Colors.Grey.Darken1);

                                    c.Item().PaddingTop(3).Text("Este documento es una representación impresa de un CFDI.").Bold().FontSize(7);
                                });
                            });
                        }

                        footer.Item().PaddingTop(5).AlignCenter().Text(x =>
                        {
                            x.Span(esFactura ? "Gracias por su preferencia - " : "Gracias por su pago - ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            });

            var pdfBytes = documento.GeneratePdf();
            return File(pdfBytes, "application/pdf");
        }

        [HttpGet("corte-caja")]
        public async Task<IActionResult> GenerarCorteCajaPdf([FromQuery] DateTime fecha, [FromQuery] string? cajero, [FromQuery] Guid tenantId)
        {
            if (tenantId == Guid.Empty) return BadRequest("Escuela no identificada");

            var escuela = await _context.Escuelas.FindAsync(tenantId);

            var query = _context.Pagos
                .Include(p => p.Alumno)
                .Where(p => p.EscuelaId == tenantId && p.Activo && p.FechaPago.Date == fecha.Date);

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
                        col.Item().Text(escuela?.Nombre ?? "ESCUELA").Bold().FontSize(18).AlignCenter().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("CORTE DE CAJA DIARIO").Bold().FontSize(14).AlignCenter();
                        col.Item().Text($"Fecha: {fecha:dd/MM/yyyy}").AlignCenter();
                        col.Item().Text($"Cajero: {(string.IsNullOrEmpty(cajero) ? "TODOS LOS CAJEROS" : cajero)}").AlignCenter();
                        col.Item().PaddingBottom(10);
                    });

                    page.Content().Column(col =>
                    {
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

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
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

        static IContainer EstiloCelda(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5);
        }

        [HttpGet("tenant")]
        public IActionResult GetTenantActual()
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Sin tenant");
            return Ok(_tenantService.TenantId.Value);
        }
    }
}