using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelacionesController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public RelacionesController(GremelikDbContext context)
        {
            _context = context;
        }

        // POST: api/Relaciones
        [HttpPost]
        public async Task<ActionResult<RelacionAlumnoTutor>> PostRelacion(RelacionAlumnoTutor relacion)
        {
            if (relacion.Usuario == null) relacion.Usuario = "Admin";

            var alumno = await _context.Alumnos.FindAsync(relacion.AlumnoId);
            var tutor = await _context.Tutores.FindAsync(relacion.TutorId);

            if (alumno == null) return BadRequest("El Alumno no existe.");
            if (tutor == null) return BadRequest("El Tutor no existe.");

            // --- AQUÍ ESTÁ TU NUEVA REGLA DE NEGOCIO ---
            if (alumno.Estatus != EstatusAlumno.Activo)
            {
                return BadRequest($"No se puede asignar un tutor. El alumno está marcado como '{alumno.Estatus}'. Solo se permiten alumnos Activos.");
            }
            // -------------------------------------------

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
