using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlumnosController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public AlumnosController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Alumnos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alumno>>> GetAlumnos()
        {
            return await _context.Alumnos
                .OrderBy(a => a.PrimerApellido) // Ordenado por apellido por defecto
                .ToListAsync();
        }

        // --- NUEVO BUSCADOR (MOTOR DE BÚSQUEDA) ---
        [HttpGet("buscar/{texto}")]
        public async Task<ActionResult<IEnumerable<Alumno>>> BuscarAlumnos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<Alumno>();

            texto = texto.Trim();

            // Buscamos coincidencias en cualquier campo clave
            // El Tenant Filter (EscuelaId) se aplica automáticamente por el DbContext
            return await _context.Alumnos
                .Where(a => a.Nombre.Contains(texto) ||
                            a.PrimerApellido.Contains(texto) ||
                            a.SegundoApellido.Contains(texto) ||
                            a.Matricula.Contains(texto) ||
                            a.CURP.Contains(texto))
                .Where(a => a.Activo) // Solo alumnos activos
                .OrderBy(a => a.PrimerApellido)
                .Take(20) // Limitamos a 20 resultados para velocidad
                .ToListAsync();
        }
        // -------------------------------------------

        [HttpGet("{id}")]
        public async Task<ActionResult<Alumno>> GetAlumno(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return NotFound();
            return alumno;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlumno(Guid id, Alumno alumno)
        {
            if (id != alumno.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            _context.Entry(alumno).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlumnoExists(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (AlumnoExists(alumno.Id)) return Conflict();
                return BadRequest("Error al actualizar. Verifique datos duplicados.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Alumno>> PostAlumno(Alumno alumno)
        {
            try
            {
                _context.Alumnos.Add(alumno);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("duplicate") == true ||
                    ex.InnerException?.Message.Contains("UNIQUE") == true)
                {
                    return BadRequest($"Ya existe un alumno con la CURP '{alumno.CURP}' en esta escuela.");
                }
                return BadRequest($"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
            }

            return CreatedAtAction("GetAlumno", new { id = alumno.Id }, alumno);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlumno(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return NotFound();

            // Validación extra sugerida: No borrar si tiene historial académico
            // (Podrías agregar un check a Inscripciones aquí)

            _context.Alumnos.Remove(alumno);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool AlumnoExists(Guid id)
        {
            return _context.Alumnos.Any(e => e.Id == id);
        }
    }
}