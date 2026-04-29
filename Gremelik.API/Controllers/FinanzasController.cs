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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class FinanzasController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public FinanzasController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // --- CONCEPTOS (AHORA INCLUYEN PRECIO Y JERARQUÍA DIRECTAMENTE) ---

        

        [HttpPost("conceptos")]
        public async Task<ActionResult<ConceptoPago>> PostConcepto(ConceptoPago concepto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            concepto.EscuelaId = _tenantService.TenantId.Value;
            concepto.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            concepto.FechaRegistro = DateTime.Now;
            concepto.Activo = true;

            // Llenamos los nombres auxiliares para que se vea bonito en la lista sin hacer joins extra
            if (concepto.PlantelId.HasValue)
            {
                var p = await _context.Planteles.FindAsync(concepto.PlantelId);
                concepto.NombrePlantel = p?.Nombre;
            }
            if (concepto.NivelEducativoId.HasValue)
            {
                var n = await _context.NivelesEducativos.FindAsync(concepto.NivelEducativoId);
                concepto.NombreNivel = n?.Nombre;
            }
            if (concepto.GradoId.HasValue)
            {
                var g = await _context.Grados.FindAsync(concepto.GradoId);
                concepto.NombreGrado = g?.Nombre;
            }

            _context.ConceptosPago.Add(concepto);
            await _context.SaveChangesAsync();
            return Ok(concepto);
        }

        [HttpPut("conceptos/{id}")]
        public async Task<IActionResult> PutConcepto(Guid id, ConceptoPago concepto)
        {
            if (id != concepto.Id) return BadRequest();

            // Actualizamos nombres auxiliares si cambiaron
            if (concepto.PlantelId.HasValue) concepto.NombrePlantel = (await _context.Planteles.FindAsync(concepto.PlantelId))?.Nombre;
            else concepto.NombrePlantel = null;

            if (concepto.NivelEducativoId.HasValue) concepto.NombreNivel = (await _context.NivelesEducativos.FindAsync(concepto.NivelEducativoId))?.Nombre;
            else concepto.NombreNivel = null;

            if (concepto.GradoId.HasValue) concepto.NombreGrado = (await _context.Grados.FindAsync(concepto.GradoId))?.Nombre;
            else concepto.NombreGrado = null;

            _context.Entry(concepto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ConceptosPago.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // --- CONCEPTOS ---
        [HttpGet("conceptos/{cicloId}")]
        public async Task<ActionResult<IEnumerable<ConceptoPago>>> GetConceptos(int cicloId)
        {
            return await _context.ConceptosPago
                .Where(c => c.CicloEscolarId == cicloId && c.Activo) // <-- SOLO ACTIVOS
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Monto)
                .ToListAsync();
        }

        [HttpDelete("conceptos/{id}")]
        public async Task<IActionResult> DeleteConcepto(Guid id)
        {
            var concepto = await _context.ConceptosPago.FindAsync(id);
            if (concepto == null) return NotFound();

            // Validar si está en uso en Planes (esta regla tuya es muy buena, la dejamos)
            bool enUso = await _context.PlanesPago.AnyAsync(p => p.ConceptoPagoId == id && p.Activo);
            if (enUso) return BadRequest("No puedes dar de baja este concepto porque es base de un Plan de Pagos activo.");

            // REGLA DE ORO: SOFT DELETE
            concepto.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- PLANES DE PAGO ---
        [HttpGet("planes/{cicloId}")]
        public async Task<ActionResult<IEnumerable<PlanPago>>> GetPlanes(int cicloId)
        {
            return await _context.PlanesPago
                .Include(p => p.ConceptoRelacionado)
                .Where(p => p.CicloEscolarId == cicloId && p.Activo) // <-- SOLO ACTIVOS
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        [HttpDelete("planes/{id}")]
        public async Task<IActionResult> DeletePlan(Guid id)
        {
            var plan = await _context.PlanesPago.FindAsync(id);
            if (plan == null) return NotFound();

            // REGLA DE ORO: SOFT DELETE
            plan.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }



        [HttpPost("planes")]
        public async Task<ActionResult<PlanPago>> PostPlan(PlanPago plan)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            plan.EscuelaId = _tenantService.TenantId.Value;
            plan.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            plan.FechaRegistro = DateTime.Now;
            plan.Activo = true;

            _context.PlanesPago.Add(plan);
            await _context.SaveChangesAsync();
            return Ok(plan);
        }

        [HttpPut("planes/{id}")]
        public async Task<IActionResult> PutPlan(Guid id, PlanPago plan)
        {
            if (id != plan.Id) return BadRequest();
            _context.Entry(plan).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        
    }
}
