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
    public class CalificacionesController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public CalificacionesController(GremelikDbContext context)
        {
            _context = context;
        }

        // --- 1. OBTENER LA MATRIZ DE CALIFICACIONES PARA LA PANTALLA ---
        [HttpGet("matriz")]
        public async Task<ActionResult> GetMatriz([FromQuery] int grupoId, [FromQuery] Guid materiaId, [FromQuery] int trimestreSep, [FromQuery] int cicloId)
        {
            var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool esMaestro = User.IsInRole("Maestro") && !User.IsInRole("GlobalAdmin") && !User.IsInRole("SchoolAdmin") && !User.IsInRole("Coordinador");

            // SEGURIDAD: Verificar que el maestro dé esta clase
            if (esMaestro)
            {
                bool daClase = await _context.AsignacionesMaestros
                    .AnyAsync(a => a.MaestroId == maestroId && a.GrupoId == grupoId && a.MateriaId == materiaId && a.CicloEscolarId == cicloId && a.Activo);
                if (!daClase) return BadRequest("No tienes permisos para calificar esta materia.");
            }

            // 1. Obtener el Nivel Educativo del Grupo para traer su Configuración
            var grupo = await _context.Grupos.Include(g => g.Grado).FirstOrDefaultAsync(g => g.Id == grupoId);
            if (grupo == null) return NotFound("Grupo no encontrado.");

            var config = await _context.ConfiguracionesAcademicas.FirstOrDefaultAsync(c => c.NivelEducativoId == grupo.Grado!.NivelEducativoId && c.Activo);
            if (config == null) return BadRequest("El coordinador aún no ha configurado las reglas de calificación para este nivel educativo.");

            // 2. Traer los Periodos (Meses/Bimestres) que pertenecen a este Trimestre SEP
            var periodos = await _context.PeriodosInternos
                .Where(p => p.NivelEducativoId == grupo.Grado!.NivelEducativoId && p.CicloEscolarId == cicloId && p.TrimestreSEP == trimestreSep && p.Activo)
                .OrderBy(p => p.Orden)
                .Select(p => new { p.Id, p.Nombre, p.AbiertoParaCaptura })
                .ToListAsync();

            if (!periodos.Any()) return BadRequest($"No hay periodos configurados para el Trimestre {trimestreSep}.");

            // 3. Traer Alumnos
            var alumnos = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Where(i => i.GrupoId == grupoId && i.Activo)
                .OrderBy(i => i.Alumno!.PrimerApellido).ThenBy(i => i.Alumno!.Nombre)
                .Select(i => new { i.AlumnoId, NombreCompleto = $"{i.Alumno!.PrimerApellido} {i.Alumno.SegundoApellido} {i.Alumno.Nombre}".Trim() })
                .ToListAsync();

            var periodoIds = periodos.Select(p => p.Id).ToList();

            // 4. Traer Calificaciones Internas capturadas
            var notasInternas = await _context.CalificacionesInternas
                .Where(c => c.GrupoId == grupoId && c.MateriaId == materiaId && periodoIds.Contains(c.PeriodoInternoId) && c.Activo)
                .ToListAsync();

            // 5. Traer Calificación SEP (Si ya está cerrada)
            var notasSEP = await _context.CalificacionesSEP
                .Where(c => c.GrupoId == grupoId && c.MateriaId == materiaId && c.CicloEscolarId == cicloId && c.Trimestre == trimestreSep && c.Activo)
                .ToListAsync();

            // 6. Ensamblar la respuesta para la tabla
            var filas = new List<object>();
            foreach (var al in alumnos)
            {
                var notasAlumno = notasInternas.Where(n => n.AlumnoId == al.AlumnoId).Select(n => new { n.PeriodoInternoId, n.Nota }).ToList();
                var sepAlumno = notasSEP.FirstOrDefault(n => n.AlumnoId == al.AlumnoId);

                filas.Add(new
                {
                    al.AlumnoId,
                    al.NombreCompleto,
                    NotasInternas = notasAlumno,
                    NotaSEP = sepAlumno?.NotaFinal,
                    Confirmado = sepAlumno?.Confirmado ?? false
                });
            }

            return Ok(new
            {
                UsaDecimales = config.UsaDecimales,
                CalificacionAprobatoria = config.CalificacionAprobatoria,
                EscalaMinima = config.EscalaMinima,
                EscalaMaxima = config.EscalaMaxima,
                Periodos = periodos,
                Alumnos = filas
            });
        }

        // --- 2. GUARDAR CALIFICACIONES DE UN MES/BIMESTRE ---
        [HttpPost("guardar-internas")]
        public async Task<IActionResult> GuardarInternas([FromBody] GuardarInternasDto dto)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            // Validar si el trimestre SEP ya está cerrado para esta materia y grupo
            // (Si ya está cerrado, no debería poder cambiar los meses que lo componen)
            var periodoInfo = await _context.PeriodosInternos.FindAsync(dto.PeriodoInternoId);
            if (periodoInfo != null)
            {
                // --- NUEVO CANDADO DE SEGURIDAD (API) ---
                if (!periodoInfo.AbiertoParaCaptura)
                {
                    return BadRequest($"El periodo '{periodoInfo.Nombre}' se encuentra cerrado por el Coordinador. No se pueden recibir calificaciones.");
                }

                bool trimestreCerrado = await _context.CalificacionesSEP.AnyAsync(c =>
                    c.GrupoId == dto.GrupoId && c.MateriaId == dto.MateriaId && c.Trimestre == periodoInfo.TrimestreSEP && c.Confirmado && c.Activo);

                if (trimestreCerrado) return BadRequest("No puedes modificar este mes porque su Trimestre SEP oficial ya fue cerrado y confirmado.");
            }

            var existentes = await _context.CalificacionesInternas
                .Where(c => c.GrupoId == dto.GrupoId && c.MateriaId == dto.MateriaId && c.PeriodoInternoId == dto.PeriodoInternoId && c.Activo)
                .ToListAsync();

            foreach (var item in dto.Calificaciones)
            {
                var registro = existentes.FirstOrDefault(c => c.AlumnoId == item.AlumnoId);
                if (registro != null)
                {
                    registro.Nota = item.Nota;
                    registro.FUM = DateTime.Now;
                    registro.Usuario = usuarioActual;
                    _context.CalificacionesInternas.Update(registro);
                }
                else
                {
                    _context.CalificacionesInternas.Add(new CalificacionInterna
                    {
                        AlumnoId = item.AlumnoId,
                        GrupoId = dto.GrupoId,
                        MateriaId = dto.MateriaId,
                        PeriodoInternoId = dto.PeriodoInternoId,
                        Nota = item.Nota,
                        Usuario = usuarioActual,
                        FechaRegistro = DateTime.Now,
                        Activo = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Calificaciones guardadas correctamente." });
        }

        // --- 3. CERRAR EL TRIMESTRE SEP ---
        // --- 3. CERRAR EL TRIMESTRE SEP ---
        [HttpPost("cerrar-sep")]
        public async Task<IActionResult> CerrarTrimestreSEP([FromBody] GuardarSEPDto dto)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            // --- NUEVAS VALIDACIONES DE SEGURIDAD ESTRICTA ---
            var grupo = await _context.Grupos.Include(g => g.Grado).FirstOrDefaultAsync(g => g.Id == dto.GrupoId);
            if (grupo == null) return BadRequest("Grupo inválido.");

            var periodosTrimestre = await _context.PeriodosInternos
                .Where(p => p.NivelEducativoId == grupo.Grado!.NivelEducativoId && p.CicloEscolarId == dto.CicloId && p.TrimestreSEP == dto.Trimestre && p.Activo)
                .ToListAsync();

            // Bloqueo 1: ¿El coordinador dejó un mes abierto?
            if (periodosTrimestre.Any(p => p.AbiertoParaCaptura))
            {
                return BadRequest("El sistema no permite cerrar el trimestre oficial porque el coordinador aún no ha cerrado todos los periodos mensuales correspondientes.");
            }

            var periodoIds = periodosTrimestre.Select(p => p.Id).ToList();
            var alumnoIds = dto.Calificaciones.Select(c => c.AlumnoId).ToList();

            var calificacionesInternas = await _context.CalificacionesInternas
                .Where(c => c.GrupoId == dto.GrupoId && c.MateriaId == dto.MateriaId && periodoIds.Contains(c.PeriodoInternoId) && alumnoIds.Contains(c.AlumnoId) && c.Activo)
                .ToListAsync();

            // Bloqueo 2: ¿A un alumno le faltan meses por capturar?
            foreach (var item in dto.Calificaciones)
            {
                var notasDeEsteAlumno = calificacionesInternas.Count(c => c.AlumnoId == item.AlumnoId);
                if (notasDeEsteAlumno < periodosTrimestre.Count)
                {
                    return BadRequest("Operación rechazada: Intentando sellar a un alumno que no tiene todas sus calificaciones internas completas.");
                }
            }
            // --- FIN DE VALIDACIONES ---

            var existentes = await _context.CalificacionesSEP
                .Where(c => c.GrupoId == dto.GrupoId && c.MateriaId == dto.MateriaId && c.CicloEscolarId == dto.CicloId && c.Trimestre == dto.Trimestre && c.Activo)
                .ToListAsync();

            foreach (var item in dto.Calificaciones)
            {
                var registro = existentes.FirstOrDefault(c => c.AlumnoId == item.AlumnoId);

                // Si el maestro ya le había dado confirmar antes, nos saltamos a este alumno
                if (registro != null && registro.Confirmado) continue;

                if (registro != null)
                {
                    registro.NotaFinal = item.NotaFinal;
                    registro.PromedioSugerido = item.PromedioSugerido;
                    registro.Confirmado = true; // Lo bloqueamos
                    registro.FUM = DateTime.Now;
                    registro.Usuario = usuarioActual;
                    _context.CalificacionesSEP.Update(registro);
                }
                else
                {
                    _context.CalificacionesSEP.Add(new CalificacionSEP
                    {
                        AlumnoId = item.AlumnoId,
                        GrupoId = dto.GrupoId,
                        MateriaId = dto.MateriaId,
                        CicloEscolarId = dto.CicloId,
                        Trimestre = dto.Trimestre,
                        PromedioSugerido = item.PromedioSugerido,
                        NotaFinal = item.NotaFinal,
                        Confirmado = true, // Lo bloqueamos
                        Usuario = usuarioActual,
                        FechaRegistro = DateTime.Now,
                        Activo = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Trimestre oficial cerrado y confirmado correctamente." });
        }
    }

    // --- DTOs AUXILIARES ---
    public class GuardarInternasDto
    {
        public int GrupoId { get; set; }
        public Guid MateriaId { get; set; }
        public Guid PeriodoInternoId { get; set; }
        public List<NotaAlumnoDto> Calificaciones { get; set; } = new();
    }

    public class GuardarSEPDto
    {
        public int GrupoId { get; set; }
        public Guid MateriaId { get; set; }
        public int CicloId { get; set; }
        public int Trimestre { get; set; }
        public List<NotaSEPDto> Calificaciones { get; set; } = new();
    }

    public class NotaAlumnoDto { public Guid AlumnoId { get; set; } public decimal Nota { get; set; } }
    public class NotaSEPDto { public Guid AlumnoId { get; set; } public decimal PromedioSugerido { get; set; } public decimal NotaFinal { get; set; } }
}