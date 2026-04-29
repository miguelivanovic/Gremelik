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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador, Maestro")]
    public class ReportesAcademicosController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService; // Inyectamos el servicio para saber qué escuela es

        public ReportesAcademicosController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("boleta/alumno/{alumnoId}/grupo/{grupoId}/ciclo/{cicloId}")]
        public async Task<ActionResult> GetBoletaCompleta(Guid alumnoId, int grupoId, int cicloId)
        {
            // 0. Traer los datos de la Escuela (Logo y Nombre)
            string nombreEscuela = "COLEGIO GREMELIK";
            string logoEscuela = "";

            if (_tenantService.TenantId.HasValue)
            {
                // NOTA: Si tu DbSet se llama diferente, ajusta "Escuelas" por tu nombre real
                var escuela = await _context.Escuelas.FindAsync(_tenantService.TenantId.Value);
                if (escuela != null)
                {
                    nombreEscuela = escuela.Nombre;
                    logoEscuela = escuela.LogoUrl ?? ""; // Ajusta el nombre de tu propiedad de logo
                }
            }

            // 1. Datos Generales del Alumno
            var inscripcion = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Grupo).ThenInclude(g => g.Grado).ThenInclude(g => g.NivelEducativo)
                .FirstOrDefaultAsync(i => i.AlumnoId == alumnoId && i.GrupoId == grupoId && i.Activo);

            if (inscripcion == null) return NotFound("El alumno no está inscrito en este grupo.");

            var ciclo = await _context.CiclosEscolares.FindAsync(cicloId);

            // 2. Traer los Periodos Internos configurados para este nivel y ciclo (Meses/Bimestres)
            var periodos = await _context.PeriodosInternos
                .Where(p => p.NivelEducativoId == inscripcion.Grupo!.Grado!.NivelEducativoId && p.CicloEscolarId == cicloId && p.Activo)
                .OrderBy(p => p.Orden)
                .Select(p => new PeriodoBoletaDto { Id = p.Id, Nombre = p.Nombre, TrimestreSEP = p.TrimestreSEP })
                .ToListAsync();

            // 3. Obtener todas las Materias
            var materiasGrupo = await _context.AsignacionesMaestros
                .Include(a => a.Materia)
                .Where(a => a.GrupoId == grupoId && a.CicloEscolarId == cicloId && a.Activo)
                .Select(a => a.Materia)
                .Distinct()
                .ToListAsync();

            // 4. Traer Calificaciones Oficiales (SEP) e Internas (Meses)
            var calificacionesSEP = await _context.CalificacionesSEP
                .Where(c => c.AlumnoId == alumnoId && c.GrupoId == grupoId && c.CicloEscolarId == cicloId && c.Activo)
                .ToListAsync();

            var calificacionesInternas = await _context.CalificacionesInternas
                .Where(c => c.AlumnoId == alumnoId && c.GrupoId == grupoId && c.Activo)
                .ToListAsync();

            // 5. Traer Faltas
            var faltas = await _context.Asistencias
                .Where(a => a.AlumnoId == alumnoId && a.GrupoId == grupoId && (int)a.Estatus == 2 && a.Activo)
                .GroupBy(a => a.MateriaId)
                .Select(g => new { MateriaId = g.Key, TotalFaltas = g.Count() })
                .ToListAsync();

            // 6. Ensamblar la Boleta
            var boleta = new BoletaAlumnoDto
            {
                EscuelaNombre = nombreEscuela,
                EscuelaLogo = logoEscuela,
                AlumnoId = alumnoId,
                NombreCompleto = $"{inscripcion.Alumno!.PrimerApellido} {inscripcion.Alumno.SegundoApellido} {inscripcion.Alumno.Nombre}".Trim(),
                // Antes decía: ... {inscripcion.Grupo.Grado.Nombre} \"{inscripcion.Grupo.Turno}\""
                // Ahora dirá: Primaria - 1ro A
                NivelGradoGrupo = $"{inscripcion.Grupo!.Grado!.NivelEducativo!.Nombre} - {inscripcion.Grupo.Grado.Nombre} {inscripcion.Grupo.Nombre}",
                CicloEscolar = ciclo?.Nombre ?? "",
                Periodos = periodos,
                Materias = new List<BoletaMateriaDto>()
            };

            decimal sumaPromedios = 0;
            int materiasEvaluadas = 0;

            foreach (var materia in materiasGrupo.OrderBy(m => m!.Nombre))
            {
                if (materia == null) continue;

                // Extraemos las notas internas de esta materia
                var notasInternasMateria = calificacionesInternas
                    .Where(c => c.MateriaId == materia.Id)
                    .Select(c => new NotaInternaBoletaDto { PeriodoId = c.PeriodoInternoId, Nota = c.Nota })
                    .ToList();

                var notasMateria = calificacionesSEP.Where(c => c.MateriaId == materia.Id).ToList();
                var t1 = notasMateria.FirstOrDefault(n => n.Trimestre == 1)?.NotaFinal;
                var t2 = notasMateria.FirstOrDefault(n => n.Trimestre == 2)?.NotaFinal;
                var t3 = notasMateria.FirstOrDefault(n => n.Trimestre == 3)?.NotaFinal;

                decimal? promedioFinal = null;
                if (t1.HasValue && t2.HasValue && t3.HasValue)
                {
                    promedioFinal = Math.Round((t1.Value + t2.Value + t3.Value) / 3, 1);
                    sumaPromedios += promedioFinal.Value;
                    materiasEvaluadas++;
                }

                boleta.Materias.Add(new BoletaMateriaDto
                {
                    Materia = materia.Nombre,
                    CampoFormativo = materia.CampoFormativo ?? "Sin Especificar",
                    NotasInternas = notasInternasMateria,
                    Trimestre1 = t1,
                    Trimestre2 = t2,
                    Trimestre3 = t3,
                    PromedioFinal = promedioFinal,
                    FaltasTotales = faltas.FirstOrDefault(f => f.MateriaId == materia.Id)?.TotalFaltas ?? 0
                });
            }

            boleta.PromedioGeneral = materiasEvaluadas > 0 ? Math.Round(sumaPromedios / materiasEvaluadas, 1) : 0;
            boleta.FaltasTotales = boleta.Materias.Sum(m => m.FaltasTotales);

            return Ok(boleta);
        }

        [HttpGet("boletas/grupo/{grupoId}/ciclo/{cicloId}")]
        public async Task<ActionResult> GetBoletasGrupoCompleto(int grupoId, int cicloId)
        {
            string nombreEscuela = "COLEGIO GREMELIK";
            string logoEscuela = "";
            if (_tenantService.TenantId.HasValue)
            {
                var escuela = await _context.Escuelas.FindAsync(_tenantService.TenantId.Value);
                if (escuela != null) { nombreEscuela = escuela.Nombre; logoEscuela = escuela.LogoUrl ?? ""; }
            }

            var inscripciones = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Grupo).ThenInclude(g => g.Grado).ThenInclude(g => g.NivelEducativo)
                .Where(i => i.GrupoId == grupoId && i.Activo)
                .ToListAsync();

            if (!inscripciones.Any()) return NotFound("No hay alumnos en este grupo.");

            var ciclo = await _context.CiclosEscolares.FindAsync(cicloId);
            var nivelId = inscripciones.First().Grupo!.Grado!.NivelEducativoId;

            var periodos = await _context.PeriodosInternos
                .Where(p => p.NivelEducativoId == nivelId && p.CicloEscolarId == cicloId && p.Activo)
                .OrderBy(p => p.Orden)
                .Select(p => new PeriodoBoletaDto { Id = p.Id, Nombre = p.Nombre, TrimestreSEP = p.TrimestreSEP })
                .ToListAsync();

            var materiasGrupo = await _context.AsignacionesMaestros
                .Include(a => a.Materia)
                .Where(a => a.GrupoId == grupoId && a.CicloEscolarId == cicloId && a.Activo)
                .Select(a => a.Materia).Distinct().ToListAsync();

            // Carga masiva de calificaciones y faltas de TODO EL GRUPO
            var sepGrupo = await _context.CalificacionesSEP.Where(c => c.GrupoId == grupoId && c.CicloEscolarId == cicloId && c.Activo).ToListAsync();
            var internasGrupo = await _context.CalificacionesInternas.Where(c => c.GrupoId == grupoId && c.Activo).ToListAsync();
            var faltasGrupo = await _context.Asistencias.Where(a => a.GrupoId == grupoId && (int)a.Estatus == 2 && a.Activo).ToListAsync();

            var listaBoletas = new List<BoletaAlumnoDto>();

            foreach (var ins in inscripciones.OrderBy(i => i.Alumno!.PrimerApellido).ThenBy(i => i.Alumno!.Nombre))
            {
                var boleta = new BoletaAlumnoDto
                {
                    EscuelaNombre = nombreEscuela,
                    EscuelaLogo = logoEscuela,
                    AlumnoId = ins.AlumnoId,
                    NombreCompleto = $"{ins.Alumno!.PrimerApellido} {ins.Alumno.SegundoApellido} {ins.Alumno.Nombre}".Trim(),
                    NivelGradoGrupo = $"{ins.Grupo!.Grado!.NivelEducativo!.Nombre} - {ins.Grupo.Grado.Nombre} {ins.Grupo.Nombre}",
                    CicloEscolar = ciclo?.Nombre ?? "",
                    Periodos = periodos,
                    Materias = new List<BoletaMateriaDto>()
                };

                decimal sumaPromedios = 0; int materiasEvaluadas = 0;

                foreach (var mat in materiasGrupo.OrderBy(m => m!.Nombre))
                {
                    if (mat == null) continue;
                    var matId = mat.Id;
                    var alId = ins.AlumnoId;

                    var internas = internasGrupo.Where(c => c.AlumnoId == alId && c.MateriaId == matId)
                        .Select(c => new NotaInternaBoletaDto { PeriodoId = c.PeriodoInternoId, Nota = c.Nota }).ToList();

                    var sep = sepGrupo.Where(c => c.AlumnoId == alId && c.MateriaId == matId).ToList();
                    var t1 = sep.FirstOrDefault(n => n.Trimestre == 1)?.NotaFinal;
                    var t2 = sep.FirstOrDefault(n => n.Trimestre == 2)?.NotaFinal;
                    var t3 = sep.FirstOrDefault(n => n.Trimestre == 3)?.NotaFinal;

                    decimal? prom = null;
                    if (t1.HasValue && t2.HasValue && t3.HasValue) { prom = Math.Round((t1.Value + t2.Value + t3.Value) / 3, 1); sumaPromedios += prom.Value; materiasEvaluadas++; }

                    boleta.Materias.Add(new BoletaMateriaDto { Materia = mat.Nombre,
                        CampoFormativo = mat.CampoFormativo ?? "Sin Especificar", NotasInternas = internas, Trimestre1 = t1, Trimestre2 = t2, Trimestre3 = t3, PromedioFinal = prom, FaltasTotales = faltasGrupo.Count(f => f.AlumnoId == alId && f.MateriaId == matId) });
                    }
                boleta.PromedioGeneral = materiasEvaluadas > 0 ? Math.Round(sumaPromedios / materiasEvaluadas, 1) : 0;
                boleta.FaltasTotales = boleta.Materias.Sum(m => m.FaltasTotales);
                listaBoletas.Add(boleta);
            }
            return Ok(listaBoletas);
        }
    }

    // --- DTOs ACTUALIZADOS ---
    public class BoletaAlumnoDto
    {
        public string EscuelaNombre { get; set; } = "";
        public string EscuelaLogo { get; set; } = "";
        public Guid AlumnoId { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string NivelGradoGrupo { get; set; } = "";
        public string CicloEscolar { get; set; } = "";
        public decimal PromedioGeneral { get; set; }
        public int FaltasTotales { get; set; }
        public List<PeriodoBoletaDto> Periodos { get; set; } = new();
        public List<BoletaMateriaDto> Materias { get; set; } = new();
    }

    public class PeriodoBoletaDto { public Guid Id { get; set; } public string Nombre { get; set; } = ""; public int TrimestreSEP { get; set; } }
    public class NotaInternaBoletaDto { public Guid PeriodoId { get; set; } public decimal Nota { get; set; } }

    public class BoletaMateriaDto
    {
        public string Materia { get; set; } = "";
        // 👇 1. AGREGA ESTA LÍNEA 👇
        public string CampoFormativo { get; set; } = "Sin Especificar";
        public List<NotaInternaBoletaDto> NotasInternas { get; set; } = new();
        public decimal? Trimestre1 { get; set; }
        public decimal? Trimestre2 { get; set; }
        public decimal? Trimestre3 { get; set; }
        public decimal? PromedioFinal { get; set; }
        public int FaltasTotales { get; set; }
    }
}