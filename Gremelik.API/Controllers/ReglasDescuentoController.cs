using Gremelik.core.Entities;
using Gremelik.core.Services; // Necesario para Tenant
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
    public class ReglasDescuentoController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService; // Inyectamos el servicio

        public ReglasDescuentoController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // GET: api/ReglasDescuento/ciclo/5
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<ReglaDescuento>>> GetPorCiclo(int cicloId)
        {
            return await _context.ReglasDescuento
                .Where(r => r.CicloEscolarId == cicloId)
                .OrderByDescending(r => r.Activo)
                .ThenBy(r => r.Nombre)
                .ToListAsync();
        }

        // POST: api/ReglasDescuento
        [HttpPost]
        public async Task<ActionResult<ReglaDescuento>> PostRegla(ReglaDescuento regla)
        {
            try
            {
                ModelState.Remove("Usuario");

                // 1. ASIGNAR ESCUELA (El error estaba aquí)
                if (_tenantService.TenantId.HasValue)
                {
                    regla.EscuelaId = _tenantService.TenantId.Value;
                }
                else
                {
                    return BadRequest("No se identificó la escuela.");
                }

                // 2. Asignar Auditoría
                var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
                regla.Usuario = usuarioActual;
                regla.FUM = DateTime.Now;

                if (regla.Id == Guid.Empty)
                {
                    // NUEVA
                    regla.FechaRegistro = DateTime.Now;
                    regla.Activo = true;
                    _context.ReglasDescuento.Add(regla);
                }
                else
                {
                    // EDITAR (Asegurando que no se pierda el EscuelaId)
                    // Truco: Adjuntamos la entidad y decimos que fue modificada
                    // pero protegemos campos clave.
                    var existente = await _context.ReglasDescuento.FindAsync(regla.Id);
                    if (existente == null) return NotFound();

                    // Actualizamos campos editables
                    existente.Nombre = regla.Nombre;
                    existente.Tipo = regla.Tipo;
                    existente.Porcentaje = regla.Porcentaje;
                    existente.MontoFijo = regla.MontoFijo;
                    existente.FechaInicioValidez = regla.FechaInicioValidez;
                    existente.FechaFinValidez = regla.FechaFinValidez;
                    existente.Activo = regla.Activo;

                    // Auditoría
                    existente.Usuario = usuarioActual;
                    existente.FUM = DateTime.Now;

                    // Nota: No tocamos EscuelaId ni FechaRegistro del existente
                }

                await _context.SaveChangesAsync();
                return Ok(regla);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al guardar regla: {ex.Message}");
            }
        }

        // DELETE: api/ReglasDescuento/guid
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var regla = await _context.ReglasDescuento.FindAsync(id);
            if (regla == null) return NotFound();

            _context.ReglasDescuento.Remove(regla);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("toggle/{id}")]
        public async Task<IActionResult> ToggleActivo(Guid id)
        {
            var regla = await _context.ReglasDescuento.FindAsync(id);
            if (regla == null) return NotFound();

            regla.Activo = !regla.Activo;
            await _context.SaveChangesAsync();
            return Ok(regla.Activo);
        }
    }
}