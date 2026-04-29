using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador, Maestro")]
    public class ReportesConductaController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public ReportesConductaController(GremelikDbContext context)
        {
            _context = context;
        }

        // --- 1. OBTENER ÚLTIMOS REPORTES DEL CICLO ---
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult> GetReportes(int cicloId)
        {
            var reportes = await _context.ReportesConducta
                .Include(r => r.Alumno)
                .Where(r => r.CicloEscolarId == cicloId && r.Activo)
                .OrderByDescending(r => r.FechaIncidencia)
                .Select(r => new {
                    r.Id,
                    r.FechaIncidencia,
                    Alumno = $"{r.Alumno!.PrimerApellido} {r.Alumno.Nombre}".Trim(),
                    r.Gravedad,
                    r.Titulo,
                    r.Estatus,
                    r.NombreReportador
                })
                .ToListAsync();

            return Ok(reportes);
        }

        // --- 2. CREAR NUEVO REPORTE ---
        [HttpPost]
        public async Task<IActionResult> CrearReporte([FromBody] CrearReporteDto dto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            // Intentamos sacar el nombre del usuario de los claims del token
            var nombreUsuario = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Personal Escolar";

            var reporte = new ReporteConducta
            {
                CicloEscolarId = dto.CicloEscolarId,
                AlumnoId = dto.AlumnoId,
                ReportadoPorId = usuarioId,
                NombreReportador = nombreUsuario,
                Gravedad = (NivelGravedad)dto.Gravedad,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                FechaIncidencia = DateTime.Now,
                Estatus = EstatusReporte.Pendiente,
                Usuario = usuarioId,
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            _context.ReportesConducta.Add(reporte);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reporte guardado exitosamente." });
        }

        // --- 3. BANDEJA DIRECTIVA (Filtros y Seguridad) ---
        [HttpGet("bandeja")]
        public async Task<ActionResult> GetBandejaReportes(
            [FromQuery] int cicloId,
            [FromQuery] int grupoId = 0,
            [FromQuery] Guid? alumnoId = null)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool esSoloMaestro = User.IsInRole("Maestro") && !User.IsInRole("GlobalAdmin") && !User.IsInRole("SchoolAdmin") && !User.IsInRole("Coordinador");

            var query = _context.ReportesConducta
                .Include(r => r.Alumno)
                .Where(r => r.CicloEscolarId == cicloId && r.Activo);

            // REGLA DE ORO: Si es maestro, rompemos todos los filtros de búsqueda y lo obligamos a ver solo sus propios reportes.
            if (esSoloMaestro)
            {
                query = query.Where(r => r.ReportadoPorId == usuarioId);
            }
            else
            {
                // Filtros exclusivos para Directivos y Coordinadores
                if (alumnoId.HasValue && alumnoId.Value != Guid.Empty)
                {
                    query = query.Where(r => r.AlumnoId == alumnoId.Value);
                }
                else if (grupoId > 0)
                {
                    // Si seleccionó un grupo, buscamos a los alumnos inscritos ahí
                    var alumnosDelGrupo = _context.Inscripciones
                        .Where(i => i.GrupoId == grupoId && i.CicloEscolarId == cicloId && i.Activo)
                        .Select(i => i.AlumnoId);

                    query = query.Where(r => alumnosDelGrupo.Contains(r.AlumnoId));
                }
            }

            var reportes = await query
                .OrderByDescending(r => r.FechaIncidencia)
                .Select(r => new {
                    r.Id,
                    r.FechaIncidencia,
                    AlumnoId = r.AlumnoId,
                    Alumno = $"{r.Alumno!.PrimerApellido} {r.Alumno.SegundoApellido} {r.Alumno.Nombre}".Trim(),
                    r.Gravedad,
                    r.Titulo,
                    r.Descripcion,
                    r.Estatus,
                    r.NombreReportador,
                    r.AccionTomada
                })
                .ToListAsync();

            return Ok(reportes);
        }

        // --- 4. CERRAR / RESOLVER EL REPORTE ---
        [HttpPut("resolver/{id}")]
        [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador")] // CANDADO: Los maestros no pueden cerrar reportes
        public async Task<IActionResult> ResolverReporte(Guid id, [FromBody] ResolverReporteDto dto)
        {
            var reporte = await _context.ReportesConducta.FindAsync(id);
            if (reporte == null || !reporte.Activo) return NotFound("Reporte no encontrado.");

            reporte.Estatus = (EstatusReporte)dto.Estatus;
            reporte.AccionTomada = dto.AccionTomada;
            reporte.FUM = DateTime.Now;
            reporte.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.ReportesConducta.Update(reporte);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Reporte actualizado correctamente." });
        }
    }

    // --- DTO AUXILIAR ---
    public class CrearReporteDto
    {
        public int CicloEscolarId { get; set; }
        public Guid AlumnoId { get; set; }
        public int Gravedad { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public class ResolverReporteDto
    {
        public int Estatus { get; set; }
        public string? AccionTomada { get; set; } // Ej. "Se suspendió 3 días al alumno."
    }
}