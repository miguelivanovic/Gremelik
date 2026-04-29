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
    public class GradosController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public GradosController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Grados/nivel/{nivelId}
        // OJO: Aquí recibimos GUID porque me confirmaste que NivelEducativoId es Guid
        // GET: api/Grados/nivel/{nivelId}
        [HttpGet("nivel/{nivelId}")]
        public async Task<ActionResult<IEnumerable<Grado>>> GetPorNivel(Guid nivelId)
        {
            var query = _context.Grados.Where(g => g.NivelEducativoId == nivelId);

            bool esMaestro = User.IsInRole("Maestro") && !User.IsInRole("GlobalAdmin") && !User.IsInRole("SchoolAdmin") && !User.IsInRole("Coordinador");

            if (esMaestro)
            {
                var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var gradosAsignados = _context.AsignacionesMaestros
                    .Include(a => a.Grupo)
                    .Where(a => a.MaestroId == maestroId && a.Activo)
                    .Select(a => a.Grupo!.GradoId)
                    .Distinct();

                query = query.Where(g => gradosAsignados.Contains(g.Id));
            }

            return await query.OrderBy(g => g.Numero).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Grado>> Post(Grado grado)
        {
            _context.Grados.Add(grado);
            await _context.SaveChangesAsync();
            return Ok(grado);
        }

        // Endpoint mágico: Crear grados en lote (Ej: Crear 1ro a 6to de un golpe)
        [HttpPost("lote")]
        public async Task<IActionResult> CrearLote([FromBody] List<Grado> grados)
        {
            _context.Grados.AddRange(grados);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var grado = await _context.Grados.FindAsync(id);
            if (grado == null) return NotFound();
            _context.Grados.Remove(grado);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
