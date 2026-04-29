using Gremelik.core.Entities;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador")]
    public class DashboardController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public DashboardController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("resumen/ciclo/{cicloId}")]
        public async Task<ActionResult<DashboardRespuestaDto>> GetResumenDashboard(int cicloId, [FromQuery] DateTime? fecha)
        {
            var hoy = fecha?.Date ?? DateTime.Now.Date;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var tenantId = _tenantService.TenantId;

            // --- 1. PULSO FINANCIERO ---
            var ingresosMes = await _context.Pagos
                .Where(p => p.Activo && p.FechaPago >= inicioMes && p.FechaPago <= finMes
                            && (!tenantId.HasValue || p.EscuelaId == tenantId.Value))
                .SumAsync(p => p.TotalPagado);

            var ingresosHoy = await _context.Pagos
                .Where(p => p.Activo && p.FechaPago.Date == hoy
                            && (!tenantId.HasValue || p.EscuelaId == tenantId.Value))
                .SumAsync(p => p.TotalPagado);

            // Cartera Vencida: Estado diferente de PAGADO o CANCELADO, haciendo la matemática manual
            var carteraVencida = await _context.CuentasPorCobrar
                .Where(c => c.Activo && c.Estado != "PAGADO" && c.Estado != "CANCELADO" && c.FechaVencimiento < hoy
                            && (!tenantId.HasValue || c.EscuelaId == tenantId.Value))
                .SumAsync(c => (c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados) - c.TotalPagado);

            // --- 2. PULSO OPERATIVO ---
            var inscripcionesActivas = await _context.Inscripciones
                .Include(i => i.Grupo).ThenInclude(g => g.Grado).ThenInclude(g => g.NivelEducativo)
                .Where(i => i.Activo && i.CicloEscolarId == cicloId)
                .ToListAsync();

            var totalAlumnos = inscripcionesActivas.Count;

            var alumnosPorNivel = inscripcionesActivas
                .Where(i => i.Grupo?.Grado?.NivelEducativo != null)
                .GroupBy(i => i.Grupo!.Grado!.NivelEducativo!.Nombre)
                .Select(g => new NivelMetricaDto { Nivel = g.Key, Total = g.Count() })
                .ToList();

            var alumnosCicloIds = inscripcionesActivas.Select(i => i.AlumnoId).Distinct().ToList();

            // Lógica corregida: Contar alumnos distintos que tienen al menos 1 falta el día de hoy
            decimal porcentajeAsistencia = 100;
            if (totalAlumnos > 0)
            {
                var alumnosAusentesHoy = await _context.Asistencias
                    .Where(a => a.Activo && a.Fecha.Date == hoy && a.Estatus == EstatusAsistencia.Falta)
                    .Select(a => a.AlumnoId)
                    .Distinct()
                    .CountAsync();

                int alumnosPresentes = totalAlumnos - alumnosAusentesHoy;
                porcentajeAsistencia = Math.Round((decimal)alumnosPresentes / totalAlumnos * 100, 1);
            }

            // --- 3. PULSO ACADÉMICO (FOCOS ROJOS) ---
            // Lógica corregida: Extraemos Faltas agrupando por Fecha para que 1 día = 1 falta
            var faltasUnicasPorDia = await _context.Asistencias
                .Include(a => a.Alumno)
                .Where(a => a.Activo && a.Estatus == EstatusAsistencia.Falta && alumnosCicloIds.Contains(a.AlumnoId))
                .Select(a => new { a.AlumnoId, a.Alumno!.Nombre, a.Alumno.PrimerApellido, Fecha = a.Fecha.Date })
                .Distinct() // Esto elimina los duplicados si faltó a varias materias el mismo día
                .ToListAsync();

            var topFaltas = faltasUnicasPorDia
                .GroupBy(a => new { a.AlumnoId, a.Nombre, a.PrimerApellido })
                .Select(g => new AlumnoRiesgoDto
                {
                    AlumnoId = g.Key.AlumnoId,
                    NombreCompleto = $"{g.Key.Nombre} {g.Key.PrimerApellido}".Trim(),
                    TotalIncidencias = g.Count(),
                    Motivo = "Faltas acumuladas"
                })
                .OrderByDescending(x => x.TotalIncidencias)
                .Take(5)
                .ToList();

            // --- ENSAMBLAR RESPUESTA ---

            // --- ENSAMBLAR RESPUESTA ---
            var dashboard = new DashboardRespuestaDto
            {
                IngresosMes = ingresosMes,
                IngresosHoy = ingresosHoy,
                CarteraVencida = carteraVencida,
                TotalAlumnos = totalAlumnos,
                PorcentajeAsistenciaHoy = porcentajeAsistencia,
                AlumnosPorNivel = alumnosPorNivel,
                AlumnosEnRiesgo = topFaltas
            };

            return Ok(dashboard);
        }
    }

    public class DashboardRespuestaDto
    {
        public decimal IngresosMes { get; set; }
        public decimal IngresosHoy { get; set; }
        public decimal CarteraVencida { get; set; }

        public int TotalAlumnos { get; set; }
        public decimal PorcentajeAsistenciaHoy { get; set; }
        public List<NivelMetricaDto> AlumnosPorNivel { get; set; } = new();

        public List<AlumnoRiesgoDto> AlumnosEnRiesgo { get; set; } = new();
    }

    public class NivelMetricaDto { public string Nivel { get; set; } = ""; public int Total { get; set; } }
    public class AlumnoRiesgoDto { public Guid AlumnoId { get; set; } public string NombreCompleto { get; set; } = ""; public int TotalIncidencias { get; set; } public string Motivo { get; set; } = ""; }
}