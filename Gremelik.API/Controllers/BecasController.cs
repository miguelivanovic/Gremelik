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
    public class BecasController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public BecasController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // CAMBIO: Filtramos por Ciclo
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<Beca>>> Get(int cicloId)
        {
            return await _context.Becas
                .Where(b => b.CicloEscolarId == cicloId)
                .OrderBy(b => b.Nombre)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Beca>> Post(Beca beca)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            beca.EscuelaId = _tenantService.TenantId.Value;
            beca.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            beca.FechaRegistro = DateTime.Now;
            beca.Activo = true;

            _context.Becas.Add(beca);
            await _context.SaveChangesAsync();

            return Ok(beca);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, Beca beca)
        {
            if (id != beca.Id) return BadRequest();
            _context.Entry(beca).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var beca = await _context.Becas.FindAsync(id);
            if (beca == null) return NotFound();
            _context.Becas.Remove(beca);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}