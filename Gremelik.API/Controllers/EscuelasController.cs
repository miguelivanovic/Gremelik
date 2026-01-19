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
    public class EscuelasController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public EscuelasController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Escuelas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Escuela>>> GetEscuelas()
        {
            return await _context.Escuelas.ToListAsync();
        }

        // GET: api/Escuelas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Escuela>> GetEscuela(Guid id)
        {
            var escuela = await _context.Escuelas.FindAsync(id);

            if (escuela == null)
            {
                return NotFound();
            }

            return escuela;
        }

        // PUT: api/Escuelas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEscuela(Guid id, Escuela escuela)
        {
            if (id != escuela.Id)
            {
                return BadRequest();
            }

            _context.Entry(escuela).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EscuelaExists(id))
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

        // POST: api/Escuelas
        [HttpPost]
        public async Task<ActionResult<Escuela>> PostEscuela(Escuela escuela)
        {
            escuela.FUM = DateTime.Now;
            if (string.IsNullOrEmpty(escuela.Usuario))
            {
                escuela.Usuario = "Sistema";
            }
            _context.Escuelas.Add(escuela);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEscuela", new { id = escuela.Id }, escuela);
        }

        // DELETE: api/Escuelas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEscuela(Guid id)
        {
            var escuela = await _context.Escuelas.FindAsync(id);
            if (escuela == null)
            {
                return NotFound();
            }

            _context.Escuelas.Remove(escuela);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EscuelaExists(Guid id)
        {
            return _context.Escuelas.Any(e => e.Id == id);
        }
    }
}