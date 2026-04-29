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
    public class RecargosController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public RecargosController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<ConfiguracionRecargo>>> Get(int cicloId)
        {
            return await _context.ConfiguracionesRecargo
                .Where(r => r.CicloEscolarId == cicloId && r.Activo)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ConfiguracionRecargo>> Post(ConfiguracionRecargo recargo)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            recargo.EscuelaId = _tenantService.TenantId.Value;
            recargo.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            recargo.FechaRegistro = DateTime.Now;
            recargo.Activo = true;

            _context.ConfiguracionesRecargo.Add(recargo);
            await _context.SaveChangesAsync();

            return Ok(recargo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ConfiguracionRecargo recargo)
        {
            if (id != recargo.Id) return BadRequest();

            recargo.FUM = DateTime.Now;
            recargo.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.Entry(recargo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recargo = await _context.ConfiguracionesRecargo.FindAsync(id);
            if (recargo == null) return NotFound();

            recargo.Activo = false;
            recargo.FUM = DateTime.Now;
            recargo.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.Entry(recargo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
