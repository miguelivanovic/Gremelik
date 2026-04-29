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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador, Maestro")]
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
            // Solo devolvemos los que están activos (El filtro de escuela lo hace tu Global Query Filter)
            return await _context.Planteles.Where(p => p.Activo).ToListAsync();
        }

        // ... (Tu método POST y PUT se quedan igualito) ...

        // DELETE: api/Planteles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlantel(Guid id)
        {
            var plantel = await _context.Planteles.FindAsync(id);
            if (plantel == null) return NotFound();

            // REGLA DE ORO: Soft Delete (Borrado Lógico) en lugar de Remove
            plantel.Activo = false;

            await _context.SaveChangesAsync();
            return NoContent();
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

        // PUT: api/Planteles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlantel(Guid id, Plantel plantelActualizado)
        {
            if (id != plantelActualizado.Id) return BadRequest("El ID no coincide.");

            var plantelDb = await _context.Planteles.FindAsync(id);
            if (plantelDb == null) return NotFound();

            // Actualizamos solo los campos permitidos
            plantelDb.Nombre = plantelActualizado.Nombre;
            plantelDb.Estado = plantelActualizado.Estado;
            plantelDb.Calle = plantelActualizado.Calle;
            plantelDb.Numero = plantelActualizado.Numero;
            plantelDb.Colonia = plantelActualizado.Colonia;
            plantelDb.Municipio = plantelActualizado.Municipio;
            plantelDb.CodigoPostal = plantelActualizado.CodigoPostal;

            // Datos Fiscales
            plantelDb.CCT = plantelActualizado.CCT;
            plantelDb.RFC = plantelActualizado.RFC;
            plantelDb.RazonSocial = plantelActualizado.RazonSocial;
            plantelDb.CodigoPostalFiscal = plantelActualizado.CodigoPostalFiscal;
            plantelDb.RegimenFiscal = plantelActualizado.RegimenFiscal;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        
    }
}
