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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")] // Solo Admins
    public class PlantelesController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public PlantelesController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // GET: api/Planteles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Plantel>>> GetPlanteles()
        {
            // El Global Query Filter ya se encarga de filtrar por escuela.
            // Así que esto solo devuelve los planteles de "mi" escuela.
            return await _context.Planteles.ToListAsync();
        }

        // POST: api/Planteles
        [HttpPost]
        public async Task<ActionResult<Plantel>> PostPlantel(Plantel plantel)
        {
            // 1. Asignación automática de la Escuela (Tenant)
            if (_tenantService.TenantId.HasValue)
            {
                plantel.EscuelaId = _tenantService.TenantId.Value;
            }
            else
            {
                // Si es GlobalAdmin creando sin estar en una escuela, esto fallaría.
                // Asumimos que estás dentro del portal de la escuela.
                return BadRequest("No se ha identificado la escuela actual.");
            }

            // 2. Si es el primer plantel, lo marcamos como Matriz
            bool existeAlguno = await _context.Planteles.AnyAsync();
            plantel.EsMatriz = !existeAlguno;

            _context.Planteles.Add(plantel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlanteles", new { id = plantel.Id }, plantel);
        }

        // DELETE: api/Planteles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlantel(Guid id)
        {
            var plantel = await _context.Planteles.FindAsync(id);
            if (plantel == null) return NotFound();

            // Validación extra: No borrar si ya tiene alumnos (pendiente)

            _context.Planteles.Remove(plantel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
