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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class NivelesController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public NivelesController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Niveles/plantel/{plantelId}
        [HttpGet("plantel/{plantelId}")]
        public async Task<ActionResult<IEnumerable<NivelEducativo>>> GetPorPlantel(Guid plantelId)
        {
            return await _context.NivelesEducativos
                .Where(n => n.PlantelId == plantelId)
                .OrderBy(n => n.Orden)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<NivelEducativo>> Post(NivelEducativo nivel)
        {
            // Validación simple: no duplicar nombres en el mismo plantel
            bool existe = await _context.NivelesEducativos
                .AnyAsync(n => n.PlantelId == nivel.PlantelId && n.Nombre == nivel.Nombre);

            if (existe) return BadRequest("Este nivel ya existe en este plantel.");

            _context.NivelesEducativos.Add(nivel);
            await _context.SaveChangesAsync();
            return Ok(nivel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var nivel = await _context.NivelesEducativos.FindAsync(id);
            if (nivel == null) return NotFound();

            _context.NivelesEducativos.Remove(nivel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}