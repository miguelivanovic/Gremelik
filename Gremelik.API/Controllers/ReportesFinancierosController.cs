using Gremelik.core.DTOs;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class ReportesFinancierosController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public ReportesFinancierosController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("corte-caja")]
        public async Task<ActionResult<CorteCajaDto>> GetCorteDeCaja([FromQuery] DateTime? fechaConsulta)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            // Si no mandan fecha, tomamos la de hoy
            var fecha = fechaConsulta?.Date ?? DateTime.Today;
            var tenantId = _tenantService.TenantId.Value;

            // 1. Traer todos los pagos del día para esta escuela
            var pagosDelDia = await _context.Pagos
                .Include(p => p.Alumno)
                .Where(p => p.EscuelaId == tenantId
                         && p.Activo
                         && p.FechaPago.Date == fecha)
                .ToListAsync();

            // 2. Armar el DTO con los cálculos matemáticos
            var dto = new CorteCajaDto
            {
                FechaConsulta = fecha,
                TotalCobrado = pagosDelDia.Sum(p => p.TotalPagado),

                // Agrupamos para saber cuánto fue en efectivo, cuánto en tarjeta, etc.
                ResumenPorMetodo = pagosDelDia
                    .GroupBy(p => p.MetodoPago.ToString())
                    .Select(g => new ResumenMetodoPagoDto
                    {
                        MetodoPago = g.Key,
                        Total = g.Sum(p => p.TotalPagado),
                        CantidadOperaciones = g.Count()
                    }).ToList(),

                // Lista detallada de recibos
                DetallePagos = pagosDelDia
                    .OrderByDescending(p => p.FechaPago)
                    .Select(p => new PagoCorteDto
                    {
                        PagoId = p.Id,
                        Folio = p.Folio.ToString("00000"),
                        FechaPago = p.FechaPago,
                        AlumnoNombre = $"{p.Alumno?.Nombre} {p.Alumno?.PrimerApellido}",
                        MetodoPago = p.MetodoPago.ToString(),
                        Total = p.TotalPagado,
                        Usuario = p.Usuario,
                        RequiereFactura = p.RequiereFactura
                    }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("morosos")]
        public async Task<ActionResult<List<AlumnoMorosoDto>>> GetMorosos()
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            var tenantId = _tenantService.TenantId.Value;
            var hoy = DateTime.Today;

            // 1. Buscamos todas las cuentas que ya vencieron y donde el Total Pagado es MENOR al costo real
            var deudasVencidas = await _context.CuentasPorCobrar
                .Include(c => c.Alumno)
                .Where(c => c.EscuelaId == tenantId
                         && c.Activo
                         && c.FechaVencimiento < hoy
                         // Matemática en SQL: Si el total pagado es menor al Monto Base - Becas + Recargos, entonces DEBE DINERO.
                         && c.TotalPagado < (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados))
                .ToListAsync();

            // 2. Agrupamos por Alumno
            var reporte = deudasVencidas
                .GroupBy(c => c.Alumno)
                .Where(g => g.Key != null)
                .Select(g => new AlumnoMorosoDto
                {
                    AlumnoId = g.Key!.Id,
                    Matricula = g.Key.Matricula,
                    NombreCompleto = $"{g.Key.Nombre} {g.Key.PrimerApellido} {g.Key.SegundoApellido}".Trim(),
                    // Calculamos la deuda exacta sumando lo que falta de cada concepto
                    DeudaTotal = g.Sum(x => (x.MontoBase - x.DescuentoBeca + x.RecargosAcumulados) - x.TotalPagado),
                    CantidadConceptosVencidos = g.Count(),
                    FechaDeudaMasAntigua = g.Min(x => x.FechaVencimiento)
                })
                .Where(m => m.DeudaTotal > 0)
                .OrderByDescending(m => m.DeudaTotal)
                .ToList();

            return Ok(reporte);
        }

        [HttpGet("ingresos-concepto")]
        public async Task<ActionResult<List<IngresoPorConceptoDto>>> GetIngresosPorConcepto(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] int? cicloId) // <--- Nuevo parámetro
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

            // 1. Iniciamos la consulta de Pagos
            var query = _context.Pagos
                .Include(p => p.Detalles)
                .Where(p => p.EscuelaId == tenantId && p.Activo && p.FechaPago >= inicio && p.FechaPago <= fin);

            // 2. Aplicamos el filtro de Ciclo si viene especificado
            if (cicloId.HasValue && cicloId.Value > 0)
            {
                query = query.Where(p => p.CicloEscolarId == cicloId.Value);
            }

            // 3. Obtenemos los detalles de esos pagos
            var detalles = await query.SelectMany(p => p.Detalles).ToListAsync();

            var reporte = detalles
                .GroupBy(d => d.ConceptoNombreSnapshot)
                .Select(g => new IngresoPorConceptoDto
                {
                    Concepto = string.IsNullOrEmpty(g.Key) ? "Concepto General" : g.Key,
                    TotalCobrado = g.Sum(x => x.MontoAbonado),
                    CantidadOperaciones = g.Count()
                })
                .OrderByDescending(r => r.TotalCobrado)
                .ToList();

            return Ok(reporte);
        }

        [HttpGet("ingresos-concepto/excel")]
        public async Task<IActionResult> ExportarIngresosExcel(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] int? cicloId) // <--- Nuevo parámetro
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

            // 1. Aplicamos la misma lógica de filtrado que en la consulta web
            var query = _context.Pagos
                .Include(p => p.Detalles)
                .Where(p => p.EscuelaId == tenantId && p.Activo && p.FechaPago >= inicio && p.FechaPago <= fin);

            if (cicloId.HasValue && cicloId.Value > 0)
            {
                query = query.Where(p => p.CicloEscolarId == cicloId.Value);
            }

            var detalles = await query.SelectMany(p => p.Detalles).ToListAsync();

            var reporte = detalles
                .GroupBy(d => d.ConceptoNombreSnapshot)
                .Select(g => new
                {
                    Concepto = string.IsNullOrEmpty(g.Key) ? "Concepto General" : g.Key,
                    TotalCobrado = g.Sum(x => x.MontoAbonado),
                    CantidadOperaciones = g.Count()
                })
                .OrderByDescending(r => r.TotalCobrado)
                .ToList();

            // 2. CREAMOS EL EXCEL (Mantenemos tu lógica de ClosedXML)
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ingresos por Concepto");

            worksheet.Cell(1, 1).Value = "REPORTE DE INGRESOS POR CONCEPTO";
            worksheet.Range(1, 1, 1, 3).Merge().Style.Font.SetBold().Font.FontSize = 14.0;
            worksheet.Cell(2, 1).Value = $"Periodo: {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 3).Merge();

            worksheet.Cell(4, 1).Value = "Concepto Cobrado";
            worksheet.Cell(4, 2).Value = "Operaciones";
            worksheet.Cell(4, 3).Value = "Total Recaudado";

            var headerRange = worksheet.Range(4, 1, 4, 3);
            headerRange.Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGray;

            int row = 5;
            foreach (var item in reporte)
            {
                worksheet.Cell(row, 1).Value = item.Concepto;
                worksheet.Cell(row, 2).Value = item.CantidadOperaciones;
                worksheet.Cell(row, 3).Value = item.TotalCobrado;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "$ #,##0.00";
                row++;
            }

            worksheet.Cell(row, 1).Value = "TOTAL GENERAL";
            worksheet.Cell(row, 1).Style.Font.SetBold();
            worksheet.Cell(row, 2).Value = reporte.Sum(x => x.CantidadOperaciones);
            worksheet.Cell(row, 2).Style.Font.SetBold();
            worksheet.Cell(row, 3).Value = reporte.Sum(x => x.TotalCobrado);
            worksheet.Cell(row, 3).Style.Font.SetBold().NumberFormat.Format = "$ #,##0.00";

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"Ingresos_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";

            return File(content, contentType, fileName);
        }

        [HttpGet("resumen-global/{cicloId}")]
        public async Task<ActionResult<ResumenFinancieroGlobalDto>> GetResumenGlobal(
            int cicloId,
            [FromQuery] Guid? plantelId,
            [FromQuery] Guid? nivelId,
            [FromQuery] int? gradoId,
            [FromQuery] int? grupoId)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var ciclo = await _context.CiclosEscolares.FindAsync(cicloId);
            if (ciclo == null) return NotFound("Ciclo no encontrado");

            // 1. QUERY BASE: Traemos las cuentas del ciclo solicitado.
            // Para poder filtrar por nivel/grado, traemos las inscripciones del alumno sin importar de qué ciclo sean (por si aún está en el ciclo pasado pagando el próximo).
            var query = _context.CuentasPorCobrar
                .Include(c => c.Alumno)
                    .ThenInclude(a => a.Inscripciones)
                .Where(c => c.EscuelaId == tenantId && c.CicloEscolarId == cicloId && c.Activo);

            // 2. APLICAR FILTROS INTELIGENTES (Ajustado)
            if (plantelId.HasValue || nivelId.HasValue || gradoId.HasValue || grupoId.HasValue)
            {
                // Buscamos que el alumno tenga una inscripción ACTIVA que coincida con los filtros, 
                // PERO sin exigir que esa inscripción sea del mismo ciclo de la deuda.
                // Esto permite que un alumno en el Ciclo 1 (Grado 1) pague su inscripción del Ciclo 2, y siga apareciendo si filtramos por "Grado 1" (su grado actual).
                query = query.Where(c => c.Alumno!.Inscripciones.Any(i =>
                    i.Activo &&
                    (!plantelId.HasValue || i.PlantelId == plantelId) &&
                    (!gradoId.HasValue || i.GradoId == gradoId) &&
                    (!grupoId.HasValue || i.GrupoId == grupoId)
                ));
            }

            var cuentas = await query.ToListAsync();
            var hoy = DateTime.Today;
            var dto = new ResumenFinancieroGlobalDto { CicloEscolarId = cicloId, CicloNombre = ciclo.Nombre };

            // 3. MATEMÁTICA GLOBAL
            foreach (var c in cuentas)
            {
                dto.TotalCobrado += c.TotalPagado;
                decimal saldoFaltante = (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado;

                if (saldoFaltante > 0)
                {
                    if (c.FechaVencimiento < hoy) dto.TotalVencido += saldoFaltante;
                    else dto.TotalPorCobrarFuturo += saldoFaltante;
                }
            }

            // 4. AGRUPACIÓN MENSUAL
            dto.DesgloseMensual = cuentas
                .GroupBy(c => new { c.FechaVencimiento.Year, c.FechaVencimiento.Month })
                .Select(g => new DesgloseFinancieroMensualDto
                {
                    NumeroMes = g.Key.Year * 100 + g.Key.Month,
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy").ToUpper(),
                    Cobrado = g.Sum(c => c.TotalPagado),
                    Vencido = g.Where(c => c.FechaVencimiento < hoy).Sum(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado),
                    Pendiente = g.Where(c => c.FechaVencimiento >= hoy).Sum(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado)
                })
                .OrderBy(x => x.NumeroMes)
                .ToList();

            return Ok(dto);
        }

        [HttpGet("resumen-global/pdf/{cicloId}")]
        public async Task<IActionResult> DescargarResumenGlobalPdf(int cicloId)
        {
            try
            {
                // 1. CONFIGURACIÓN DE LICENCIA (AQUÍ ESTABA EL ERROR 500 MÁS PROBABLE)
                QuestPDF.Settings.License = LicenseType.Community;

                // 2. OBTENER DATOS DIRECTAMENTE (Forma más segura que leer el ActionResult)
                if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
                var tenantId = _tenantService.TenantId.Value;

                var ciclo = await _context.CiclosEscolares.FindAsync(cicloId);
                if (ciclo == null) return NotFound("Ciclo no encontrado");

                var cuentas = await _context.CuentasPorCobrar
                    .Where(c => c.EscuelaId == tenantId && c.CicloEscolarId == cicloId && c.Activo)
                    .ToListAsync();

                var hoy = DateTime.Today;
                var datos = new ResumenFinancieroGlobalDto { CicloEscolarId = cicloId, CicloNombre = ciclo.Nombre };

                foreach (var c in cuentas)
                {
                    datos.TotalCobrado += c.TotalPagado;
                    decimal saldoFaltante = (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado;
                    if (saldoFaltante > 0)
                    {
                        if (c.FechaVencimiento < hoy) datos.TotalVencido += saldoFaltante;
                        else datos.TotalPorCobrarFuturo += saldoFaltante;
                    }
                }

                datos.DesgloseMensual = cuentas
                    .GroupBy(c => new { c.FechaVencimiento.Year, c.FechaVencimiento.Month })
                    .Select(g => new DesgloseFinancieroMensualDto
                    {
                        NumeroMes = g.Key.Year * 100 + g.Key.Month,
                        Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy").ToUpper(),
                        Cobrado = g.Sum(c => c.TotalPagado),
                        Vencido = g.Where(c => c.FechaVencimiento < hoy).Sum(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado),
                        Pendiente = g.Where(c => c.FechaVencimiento >= hoy).Sum(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado)
                    })
                    .OrderBy(x => x.NumeroMes)
                    .ToList();

                // 3. DISEÑO DEL PDF
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
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("ESTADO DE RESULTADOS FINANCIEROS").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                                col.Item().Text($"Ciclo Escolar: {datos.CicloNombre}");
                                col.Item().Text($"Fecha de Emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                            });
                        });

                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("INGRESO COBRADO").FontSize(10).FontColor(Colors.Grey.Medium);
                                    c.Item().Text($"${datos.TotalCobrado:N2}").Bold().FontSize(16).FontColor(Colors.Green.Medium);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("CARTERA VENCIDA").FontSize(10).FontColor(Colors.Grey.Medium);
                                    c.Item().Text($"${datos.TotalVencido:N2}").Bold().FontSize(16).FontColor(Colors.Red.Medium);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                                {
                                    c.Item().Text("POR COBRAR (FUTURO)").FontSize(10).FontColor(Colors.Grey.Medium);
                                    c.Item().Text($"${datos.TotalPorCobrarFuturo:N2}").Bold().FontSize(16).FontColor(Colors.Orange.Medium);
                                });
                            });

                            col.Item().PaddingTop(15).Text($"VALOR TOTAL DEL CICLO: ${datos.ValorTotalCiclo:N2}").Bold().FontSize(14).AlignRight();
                            col.Item().Height(20);

                            col.Item().PaddingBottom(5).Text("Desglose Mensual de Operaciones").Bold().FontSize(12);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Mes de Cobro").Bold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Cobrado").Bold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Vencido").Bold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Pendiente").Bold();
                                });

                                foreach (var m in datos.DesgloseMensual)
                                {
                                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(m.Mes);
                                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().Text($"${m.Cobrado:N2}").FontColor(Colors.Green.Darken1);
                                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().Text($"${m.Vencido:N2}").FontColor(Colors.Red.Medium);
                                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().Text($"${m.Pendiente:N2}");
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(x => { x.Span("Reporte Confidencial - Página "); x.CurrentPageNumber(); });
                    });
                });

                return File(documento.GeneratePdf(), "application/pdf");
            }
            catch (Exception ex)
            {
                // Si vuelve a fallar, nos escupirá el error exacto a la pantalla web
                return StatusCode(500, $"Error interno al generar el PDF: {ex.Message} | StackTrace: {ex.StackTrace}");
            }
        }

        [HttpGet("desglose-mensual/{cicloId}")]
        public async Task<ActionResult<List<ConceptoFinancieroMensualDto>>> GetDesgloseMensualPorConcepto(
            int cicloId,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] Guid? plantelId,
            [FromQuery] Guid? nivelId,
            [FromQuery] int? gradoId)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var query = _context.CuentasPorCobrar
                .Include(c => c.Alumno).ThenInclude(a => a.Inscripciones)
                .Where(c => c.EscuelaId == tenantId && c.CicloEscolarId == cicloId && c.Activo
                         && c.FechaVencimiento.Year == anio && c.FechaVencimiento.Month == mes);

            if (plantelId.HasValue || nivelId.HasValue || gradoId.HasValue)
            {
                // Mismo ajuste: quitamos la restricción de que la inscripción deba ser del mismo ciclo.
                query = query.Where(c => c.Alumno!.Inscripciones.Any(i =>
                    i.Activo &&
                    (!plantelId.HasValue || i.PlantelId == plantelId) &&
                    (!gradoId.HasValue || i.GradoId == gradoId)
                ));
            }

            var cuentas = await query.ToListAsync();
            var hoy = DateTime.Today;

            var reporte = cuentas
                .GroupBy(c => string.IsNullOrEmpty(c.ConceptoNombre) ? "Concepto General" : c.ConceptoNombre)
                .Select(g => new ConceptoFinancieroMensualDto
                {
                    Concepto = g.Key,
                    Cobrado = g.Sum(c => c.TotalPagado),
                    Vencido = g.Where(c => c.FechaVencimiento < hoy).Sum(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado),
                    Pendiente = g.Where(c => c.FechaVencimiento >= hoy).Sum(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado)
                })
                .OrderByDescending(x => x.TotalEsperado)
                .ToList();

            return Ok(reporte);
        }

        [HttpGet("auditoria-sin-cargos")]
        public async Task<ActionResult<IEnumerable<object>>> GetAlumnosSinCargos(
            [FromQuery] int cicloId,
            [FromQuery] Guid? plantelId,
            [FromQuery] Guid? nivelId,
            [FromQuery] int? gradoId,
            [FromQuery] int? grupoId)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            // 1. REGLAS DE NEGOCIO DEL CICLO
            // A) Obtener los IDs de los conceptos que pertenecen a un Plan de Colegiatura
            var conceptosDePlanes = await _context.PlanesPago
                .Where(p => p.CicloEscolarId == cicloId && p.Activo)
                .Select(p => p.ConceptoPagoId)
                .ToListAsync();

            // B) Obtener los conceptos marcados como "Obligatorios" (Libros, Uniformes, etc.)
            var conceptosObligatorios = await _context.ConceptosPago
                .Where(c => c.EscuelaId == tenantId && c.CicloEscolarId == cicloId && c.Obligatorio && c.Activo)
                .Select(c => new { c.Id, c.GradoId })
                .ToListAsync();

            // 2. BUSCAMOS INSCRIPCIONES ACTIVAS (Igual que antes)
            var query = _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Grupo).ThenInclude(g => g.Grado).ThenInclude(gr => gr.NivelEducativo)
                .Include(i => i.Grado).ThenInclude(gr => gr.NivelEducativo)
                .Where(i => i.Alumno!.EscuelaId == tenantId && i.CicloEscolarId == cicloId && i.Activo);

            if (plantelId.HasValue && plantelId != Guid.Empty) query = query.Where(i => i.PlantelId == plantelId);
            if (grupoId.HasValue && grupoId > 0) query = query.Where(i => i.GrupoId == grupoId);
            else if (gradoId.HasValue && gradoId > 0) query = query.Where(i => i.GradoId == gradoId || (i.Grupo != null && i.Grupo.GradoId == gradoId));

            var inscripciones = await query.ToListAsync();

            // 3. OBTENEMOS TODOS LOS CARGOS DE ESTOS ALUMNOS EN ESTE CICLO
            var alumnoIds = inscripciones.Select(i => i.AlumnoId).ToList();
            var cargosActuales = await _context.CuentasPorCobrar
                .Where(c => c.CicloEscolarId == cicloId && c.Activo && alumnoIds.Contains(c.AlumnoId))
                .Select(c => new { c.AlumnoId, c.ConceptoPagoId })
                .ToListAsync();

            // 4. LA AUDITORÍA INTELIGENTE (En memoria para máxima velocidad)
            var listaAuditoria = new List<object>();

            foreach (var ins in inscripciones)
            {
                // ¿Qué le hemos cobrado a este alumno?
                var conceptosDelAlumno = cargosActuales
                    .Where(c => c.AlumnoId == ins.AlumnoId)
                    .Select(c => c.ConceptoPagoId)
                    .ToHashSet(); // Usamos HashSet porque las búsquedas internas son súper rápidas

                // Prueba A: ¿Tiene algún cargo que pertenezca a un plan de colegiatura?
                bool tienePlanColegiatura = conceptosDePlanes.Any(planConceptoId => conceptosDelAlumno.Contains(planConceptoId));

                // Prueba B: ¿Tiene todos los conceptos obligatorios de SU grado?
                int gradoDelAlumno = ins.Grupo?.GradoId ?? ins.GradoId ?? 0;
                var obligatoriosParaEsteAlumno = conceptosObligatorios
                    .Where(c => c.GradoId == null || c.GradoId == 0 || c.GradoId == gradoDelAlumno)
                    .Select(c => c.Id)
                    .ToList();

                bool tieneTodosLosObligatorios = obligatoriosParaEsteAlumno.All(obsId => conceptosDelAlumno.Contains(obsId));

                // Si falla CUALQUIERA de las pruebas, se va a la lista de morosos operativos
                if (!tienePlanColegiatura || !tieneTodosLosObligatorios)
                {
                    var motivos = new List<string>();

                    // Si no tiene NADA
                    if (!conceptosDelAlumno.Any()) motivos.Add("Sin cargos generados");
                    else
                    {
                        if (!tienePlanColegiatura) motivos.Add("Falta Plan de Pagos");
                        if (!tieneTodosLosObligatorios) motivos.Add("Faltan Cargos Obligatorios");
                    }

                    listaAuditoria.Add(new
                    {
                        AlumnoId = ins.AlumnoId,
                        Matricula = ins.Alumno!.Matricula,
                        NombreCompleto = $"{ins.Alumno.PrimerApellido} {ins.Alumno.SegundoApellido} {ins.Alumno.Nombre}".Trim(),
                        Nivel = ins.Grupo != null ? ins.Grupo.Grado!.NivelEducativo!.Nombre : (ins.Grado != null ? ins.Grado.NivelEducativo!.Nombre : "Sin Nivel"),
                        Grado = ins.Grupo != null ? ins.Grupo.Grado!.Nombre : (ins.Grado != null ? ins.Grado.Nombre : "Sin Grado"),
                        Grupo = ins.Grupo != null ? ins.Grupo.Nombre : "Sin Grupo",
                        Motivo = string.Join(" / ", motivos) // <--- ESTO LE DIRÁ A LA CAJERA QUÉ HACER
                    });
                }
            }

            return Ok(listaAuditoria.OrderBy(a => ((dynamic)a).NombreCompleto));
        }
    }
}