using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return await _context.Alumnos.ToListAsync();
        }

        // GET: api/Alumnos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Alumno>> GetAlumno(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);

            if (alumno == null)
            {
                return NotFound();
            }

            return alumno;
        }

        // PUT: api/Alumnos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlumno(Guid id, Alumno alumno)
        {
            if (id != alumno.Id)
            {
                return BadRequest();
            }

            _context.Entry(alumno).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlumnoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Alumnos
        [HttpPost]
        public async Task<ActionResult<Alumno>> PostAlumno(Alumno alumno)
        {
            // Validaciones básicas de datos (opcional)
            if (alumno.Usuario == null) alumno.Usuario = "Admin";
            alumno.FUM = DateTime.Now;

            try
            {
                _context.Alumnos.Add(alumno);
                await _context.SaveChangesAsync(); // <-- Aquí es donde la BD puede gritar "¡Error!"
            }
            catch (DbUpdateException) // <-- Atrapamos el grito de la BD
            {
                // Si falla, verificamos si es por la CURP duplicada
                if (AlumnoExists(alumno.Id))
                {
                    return Conflict();
                }
                else
                {
                    // Mensaje personalizado para el usuario
                    return BadRequest("Error: Ya existe un alumno con esa CURP en esta escuela.");
                }
            }

            return CreatedAtAction("GetAlumno", new { id = alumno.Id }, alumno);
        }

        // DELETE: api/Alumnos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlumno(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null)
            {
                return NotFound();
            }

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