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
    public class CiclosController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public CiclosController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CicloEscolar>>> Get()
        {
            // Ordenamos: Primero el Próximo, luego Actual, luego Finalizados
            return await _context.CiclosEscolares
                .OrderBy(c => c.Estatus)
                .ThenByDescending(c => c.FechaInicio)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CicloEscolar>> Post(CicloEscolar ciclo)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("No se detectó la escuela.");

            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            ciclo.EscuelaId = _tenantService.TenantId.Value;

            // REGLA: Al crear, siempre nace como PRÓXIMO (a menos que sea el primero de todos)
            bool existeAlguno = await _context.CiclosEscolares.AnyAsync();
            if (!existeAlguno)
            {
                ciclo.Estatus = EstatusCiclo.Actual; // Si es el primero, nace activo
            }
            else
            {
                ciclo.Estatus = EstatusCiclo.Proximo; // Los demás nacen en espera
            }

            // Auditoría (Si tu clase hereda de BaseEntity)
            ciclo.Usuario = usuarioActual;
            ciclo.FechaRegistro = DateTime.Now;
            ciclo.Activo = true;

            _context.CiclosEscolares.Add(ciclo);
            await _context.SaveChangesAsync();

            return Ok(ciclo);
        }

        // Endpoint para PROMOVER (El Efecto Dominó)
        [HttpPut("activar/{id}")]
        public async Task<IActionResult> ActivarCiclo(int id)
        {
            var nuevoActual = await _context.CiclosEscolares.FindAsync(id);
            if (nuevoActual == null) return NotFound();

            // Solo permitimos activar si estaba como PROXIMO
            if (nuevoActual.Estatus != EstatusCiclo.Proximo)
                return BadRequest("Solo puedes activar ciclos con estatus 'Próximo'.");

            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscar el que actualmente es ACTUAL y pasarlo a FINALIZADO
                var viejoActual = await _context.CiclosEscolares
                    .FirstOrDefaultAsync(c => c.Estatus == EstatusCiclo.Actual);

                if (viejoActual != null)
                {
                    viejoActual.Estatus = EstatusCiclo.Finalizado;
                    // viejoActual.FUM = DateTime.Now; // Si tienes auditoría
                }

                // 2. El nuevo pasa a ACTUAL
                nuevoActual.Estatus = EstatusCiclo.Actual;
                // nuevoActual.FUM = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Ciclo actualizado correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Error al cambiar ciclo: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ciclo = await _context.CiclosEscolares.FindAsync(id);
            if (ciclo == null) return NotFound();

            // Validación: No borrar si es el actual
            if (ciclo.Estatus == EstatusCiclo.Actual)
                return BadRequest("No puedes borrar el ciclo activo. Finalízalo primero.");

            // Validación: No borrar si tiene alumnos inscritos (Integridad)
            bool tieneInscripciones = await _context.Inscripciones.AnyAsync(i => i.CicloEscolarId == id);
            if (tieneInscripciones)
                return BadRequest("No puedes borrar este ciclo porque tiene alumnos inscritos.");

            _context.CiclosEscolares.Remove(ciclo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}