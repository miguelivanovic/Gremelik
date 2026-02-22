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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class CostosInscripcionController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public CostosInscripcionController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/CostosInscripcion/ciclo/5
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<CostoInscripcion>>> GetPorCiclo(int cicloId)
        {
            // PROYECCIÓN MANUAL CORREGIDA
            var lista = await _context.CostosInscripcion
                .Where(c => c.CicloEscolarId == cicloId)
                .Select(c => new CostoInscripcion
                {
                    Id = c.Id,
                    // ESTE SÍ LO LLEVA (Es BaseEntity)
                    Usuario = c.Usuario,

                    Monto = c.Monto,
                    Concepto = c.Concepto,

                    CicloEscolarId = c.CicloEscolarId,
                    NivelEducativoId = c.NivelEducativoId,
                    GradoId = c.GradoId,

                    FechaRegistro = c.FechaRegistro,
                    Activo = c.Activo,

                    // AQUÍ ESTABA EL ERROR: Quitamos 'Usuario = ...' de estos dos
                    NivelEducativo = c.NivelEducativo != null ? new NivelEducativo
                    {
                        Nombre = c.NivelEducativo.Nombre
                    } : null,

                    Grado = c.Grado != null ? new Grado
                    {
                        Nombre = c.Grado.Nombre,
                        Numero = c.Grado.Numero
                    } : null
                })
                .ToListAsync();

            return Ok(lista);
        }

        // POST: api/CostosInscripcion
        [HttpPost]
        public async Task<ActionResult<CostoInscripcion>> PostCosto(CostoInscripcion costo)
        {
            try
            {
                // 1. OMITIR VALIDACIÓN AUTOMÁTICA DE "USUARIO"
                // Como 'Usuario' es required, el sistema rechaza la petición si viene vacío.
                // Nosotros lo llenamos manual aquí, así que limpiamos ese error.
                ModelState.Remove("Usuario");

                var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

                // 2. BUSCAR DUPLICADOS
                var query = _context.CostosInscripcion
                    .Where(c => c.CicloEscolarId == costo.CicloEscolarId);

                if (costo.GradoId != null)
                {
                    // Si es precio por grado, buscamos coincidencia exacta de grado
                    query = query.Where(c => c.GradoId == costo.GradoId);
                }
                else
                {
                    // Si es precio por nivel, buscamos que sea del mismo nivel y SIN grado
                    query = query.Where(c => c.NivelEducativoId == costo.NivelEducativoId && c.GradoId == null);
                }

                var existente = await query.FirstOrDefaultAsync();

                if (existente != null)
                {
                    // --- ACTUALIZAR ---
                    existente.Monto = costo.Monto;
                    existente.Concepto = costo.Concepto;
                    existente.Usuario = usuarioActual;
                    existente.FUM = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Ok(existente);
                }
                else
                {
                    // --- CREAR NUEVO ---
                    // Aseguramos que los IDs no sean nulos si son requeridos
                    if (costo.NivelEducativoId == null && costo.GradoId == null)
                        return BadRequest("Debes especificar un Nivel o un Grado.");

                    costo.Usuario = usuarioActual;
                    costo.FechaRegistro = DateTime.Now;
                    costo.Activo = true;

                    // Limpiamos objetos de navegación para evitar conflictos
                    costo.NivelEducativo = null;
                    costo.Grado = null;
                    costo.CicloEscolar = null;

                    _context.CostosInscripcion.Add(costo);
                    await _context.SaveChangesAsync();
                    return Ok(costo);
                }
            }
            catch (Exception ex)
            {
                // Esto te devolverá un mensaje legible en la alerta en lugar de HTML
                return StatusCode(500, $"Error interno: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var costo = await _context.CostosInscripcion.FindAsync(id);
            if (costo == null) return NotFound();

            _context.CostosInscripcion.Remove(costo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}