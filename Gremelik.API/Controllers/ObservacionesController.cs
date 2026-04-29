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
    public class ObservacionesController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public ObservacionesController(GremelikDbContext context)
        {
            _context = context;
        }

        [HttpGet("lista")]
        public async Task<ActionResult> GetListaObservaciones([FromQuery] int grupoId, [FromQuery] Guid materiaId)
        {
            var inscripciones = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Where(i => i.GrupoId == grupoId && i.Activo)
                .OrderBy(i => i.Alumno!.PrimerApellido).ThenBy(i => i.Alumno!.Nombre)
                .ToListAsync();

            var observacionesGuardadas = await _context.Observaciones
                .Where(o => o.GrupoId == grupoId && o.MateriaId == materiaId && o.Activo)
                .ToListAsync();

            var listaResult = new List<object>();

            foreach (var ins in inscripciones)
            {
                var obsPrevia = observacionesGuardadas.FirstOrDefault(o => o.AlumnoId == ins.AlumnoId);

                listaResult.Add(new
                {
                    AlumnoId = ins.AlumnoId,
                    NombreCompleto = $"{ins.Alumno!.PrimerApellido} {ins.Alumno.SegundoApellido} {ins.Alumno.Nombre}".Trim(),
                    Matricula = ins.Alumno.Matricula,
                    Notas = obsPrevia?.Notas ?? ""
                });
            }

            return Ok(listaResult);
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarObservaciones([FromBody] GuardarObservacionesDto dto)
        {
            var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existentes = await _context.Observaciones
                    .Where(o => o.GrupoId == dto.GrupoId && o.MateriaId == dto.MateriaId && o.Activo)
                    .ToListAsync();

                foreach (var alumnoInfo in dto.Alumnos)
                {
                    var registro = existentes.FirstOrDefault(o => o.AlumnoId == alumnoInfo.AlumnoId);

                    if (registro != null)
                    {
                        // Solo actualiza si el maestro cambió el texto
                        if (registro.Notas != alumnoInfo.Notas)
                        {
                            registro.Notas = alumnoInfo.Notas ?? "";
                            registro.FUM = DateTime.Now;
                            _context.Observaciones.Update(registro);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(alumnoInfo.Notas))
                    {
                        _context.Observaciones.Add(new ObservacionAlumno
                        {
                            AlumnoId = alumnoInfo.AlumnoId,
                            GrupoId = dto.GrupoId,
                            MateriaId = dto.MateriaId,
                            Notas = alumnoInfo.Notas,
                            Usuario = maestroId,
                            FechaRegistro = DateTime.Now,
                            Activo = true
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Observaciones guardadas correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }
    }

    public class GuardarObservacionesDto
    {
        public int GrupoId { get; set; }
        public Guid MateriaId { get; set; }
        public List<ObservacionAlumnoDto> Alumnos { get; set; } = new();
    }

    public class ObservacionAlumnoDto
    {
        public Guid AlumnoId { get; set; }
        public string Notas { get; set; } = "";
    }
}
