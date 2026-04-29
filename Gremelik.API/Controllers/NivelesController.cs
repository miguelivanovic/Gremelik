using Gremelik.core.Entities;
using Gremelik.core.Services;
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
    public class NivelesController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public NivelesController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Niveles/plantel/{plantelId}
        // GET: api/Niveles/plantel/{plantelId}
        [HttpGet("plantel/{plantelId}")]
        public async Task<ActionResult<IEnumerable<NivelEducativo>>> GetPorPlantel(Guid plantelId)
        {
            var query = _context.NivelesEducativos.Where(n => n.PlantelId == plantelId);

            bool esMaestro = User.IsInRole("Maestro") && !User.IsInRole("GlobalAdmin") && !User.IsInRole("SchoolAdmin") && !User.IsInRole("Coordinador");

            if (esMaestro)
            {
                var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var nivelesAsignados = _context.AsignacionesMaestros
                    .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                    .Where(a => a.MaestroId == maestroId && a.Activo)
                    .Select(a => a.Grupo!.Grado!.NivelEducativoId)
                    .Distinct();

                query = query.Where(n => nivelesAsignados.Contains(n.Id));
            }

            return await query.OrderBy(n => n.Orden).ToListAsync();
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