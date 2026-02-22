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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class RelacionesController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public RelacionesController(GremelikDbContext context)
        {
            _context = context;
            _context = context;
        }

        // POST: api/Relaciones
        [HttpPost]
        public async Task<ActionResult<RelacionAlumnoTutor>> PostRelacion(RelacionAlumnoTutor relacion)
        {
            // 1. CORRECCIÓN: Asignar Usuario OBLIGATORIO
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin";
            relacion.Usuario = usuarioActual;

            // Asignar los nuevos campos de BaseEntity
            relacion.FechaRegistro = DateTime.Now;
            relacion.Activo = true;

            // 2. Buscamos las entidades para validar
            var alumno = await _context.Alumnos.FindAsync(relacion.AlumnoId);
            var tutor = await _context.Tutores.FindAsync(relacion.TutorId);

            if (alumno == null) return BadRequest("El Alumno no existe.");
            if (tutor == null) return BadRequest("El Tutor no existe.");

            // 3. TU REGLA DE NEGOCIO (LA CONSERVAMOS)
            // Nota: Asumo que EstatusAlumno es un Enum o String que tienes definido.
            // Si te marca error aquí, verifica que 'Estatus' exista en tu clase Alumno actual.
            /* SI ALUMNO.ESTATUS TE MARCA ERROR:
               Es posible que al copiar mi clase Alumno antes, se haya borrado la propiedad 'Estatus'.
               Si es así, avísame para agregarla al modelo Alumno.
            */
            // if (alumno.Estatus != EstatusAlumno.Activo) ... (Tu código original)

            // Si usas un booleano 'Activo' en lugar de Enum:
            if (!alumno.Activo)
            {
                return BadRequest($"No se puede asignar un tutor. El alumno no está activo.");
            }

            _context.RelacionAlumnoTutor.Add(relacion);
            await _context.SaveChangesAsync();

            return Ok(relacion);
        }

        // GET: api/Relaciones/PorAlumno/{alumnoId}
        [HttpGet("PorAlumno/{alumnoId}")]
        public async Task<ActionResult<IEnumerable<RelacionAlumnoTutor>>> GetTutoresDeAlumno(Guid alumnoId)
        {
            return await _context.RelacionAlumnoTutor
                .Where(r => r.AlumnoId == alumnoId)
                .ToListAsync();
        }
    }
}