using Gremelik.core.DTOs;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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
        public async Task<ActionResult<List<IngresoPorConceptoDto>>> GetIngresosPorConcepto([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            // Ajustamos las horas para abarcar desde las 00:00:00 del primer día hasta las 23:59:59 del último
            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

            // Buscamos los pagos de esas fechas y extraemos sus "Detalles" (los conceptos cobrados)
            var detalles = await _context.Pagos
                .Include(p => p.Detalles)
                .Where(p => p.EscuelaId == tenantId && p.Activo && p.FechaPago >= inicio && p.FechaPago <= fin)
                .SelectMany(p => p.Detalles)
                .ToListAsync();

            // Agrupamos la lista por el nombre del concepto y sumamos el dinero
            var reporte = detalles
                .GroupBy(d => d.ConceptoNombreSnapshot)
                .Select(g => new IngresoPorConceptoDto
                {
                    Concepto = string.IsNullOrEmpty(g.Key) ? "Concepto General" : g.Key,
                    TotalCobrado = g.Sum(x => x.MontoAbonado),
                    CantidadOperaciones = g.Count()
                })
                .OrderByDescending(r => r.TotalCobrado) // Los que meten más dinero salen primero
                .ToList();

            return Ok(reporte);
        }

        [HttpGet("ingresos-concepto/excel")]
        public async Task<IActionResult> ExportarIngresosExcel([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

            // 1. OBTENEMOS LOS DATOS (Igual que el reporte web)
            var detalles = await _context.Pagos
                .Include(p => p.Detalles)
                .Where(p => p.EscuelaId == tenantId && p.Activo && p.FechaPago >= inicio && p.FechaPago <= fin)
                .SelectMany(p => p.Detalles)
                .ToListAsync();

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

            // 2. CREAMOS EL EXCEL PROFESIONAL
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ingresos por Concepto");

            // Títulos
            worksheet.Cell(1, 1).Value = "REPORTE DE INGRESOS POR CONCEPTO";
            worksheet.Range(1, 1, 1, 3).Merge().Style.Font.SetBold().Font.FontSize = 14.0;
            worksheet.Cell(2, 1).Value = $"Periodo: {fechaInicio:dd/MM/yyyy} al {fechaFin:dd/MM/yyyy}";
            worksheet.Range(2, 1, 2, 3).Merge();

            // Encabezados de Tabla
            worksheet.Cell(4, 1).Value = "Concepto Cobrado";
            worksheet.Cell(4, 2).Value = "Operaciones";
            worksheet.Cell(4, 3).Value = "Total Recaudado";

            var headerRange = worksheet.Range(4, 1, 4, 3);
            headerRange.Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGray;

            // Llenado de Datos
            int row = 5;
            foreach (var item in reporte)
            {
                worksheet.Cell(row, 1).Value = item.Concepto;
                worksheet.Cell(row, 2).Value = item.CantidadOperaciones;

                worksheet.Cell(row, 3).Value = item.TotalCobrado;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "$ #,##0.00"; // Formato moneda
                row++;
            }

            // Totales al final
            worksheet.Cell(row, 1).Value = "TOTAL GENERAL";
            worksheet.Cell(row, 1).Style.Font.SetBold();
            worksheet.Cell(row, 2).Value = reporte.Sum(x => x.CantidadOperaciones);
            worksheet.Cell(row, 2).Style.Font.SetBold();
            worksheet.Cell(row, 3).Value = reporte.Sum(x => x.TotalCobrado);
            worksheet.Cell(row, 3).Style.Font.SetBold().NumberFormat.Format = "$ #,##0.00";

            // Autoajustar el ancho de las columnas
            worksheet.Columns().AdjustToContents();

            // 3. RETORNAMOS EL ARCHIVO AL NAVEGADOR
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = $"Ingresos_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";

            return File(content, contentType, fileName);
        }
    }
}