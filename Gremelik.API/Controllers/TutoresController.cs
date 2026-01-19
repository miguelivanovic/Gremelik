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
    public class TutoresController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public TutoresController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Tutores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tutor>>> GetTutores()
        {
            return await _context.Tutores.ToListAsync();
        }

        // GET: api/Tutores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tutor>> GetTutor(Guid id)
        {
            var tutor = await _context.Tutores.FindAsync(id);

            if (tutor == null)
            {
                return NotFound();
            }

            return tutor;
        }

        // PUT: api/Tutores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTutor(Guid id, Tutor tutor)
        {
            if (id != tutor.Id)
            {
                return BadRequest();
            }

            _context.Entry(tutor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TutorExists(id))
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

        // POST: api/Tutores
        [HttpPost]
        public async Task<ActionResult<Tutor>> PostTutor(Tutor tutor)
        {
            tutor.FUM = DateTime.Now;
            if (string.IsNullOrEmpty(tutor.Usuario))
            {
                tutor.Usuario = "Sistema";
            }
            _context.Tutores.Add(tutor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTutor", new { id = tutor.Id }, tutor);
        }

        // DELETE: api/Tutores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTutor(Guid id)
        {
            var tutor = await _context.Tutores.FindAsync(id);
            if (tutor == null)
            {
                return NotFound();
            }

            _context.Tutores.Remove(tutor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TutorExists(Guid id)
        {
            return _context.Tutores.Any(e => e.Id == id);
        }
    }
}