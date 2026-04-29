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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador")]
    public class ConfiguracionAcademicaController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public ConfiguracionAcademicaController(GremelikDbContext context)
        {
            _context = context;
        }

        // --- 1. OBTENER CONFIGURACIÓN Y PERIODOS ---
        [HttpGet("nivel/{nivelId}/ciclo/{cicloId}")]
        public async Task<ActionResult> GetConfiguracionCompleta(Guid nivelId, int cicloId)
        {
            // 1. Buscamos la regla general del Nivel
            var config = await _context.ConfiguracionesAcademicas
                .FirstOrDefaultAsync(c => c.NivelEducativoId == nivelId && c.Activo);

            // 2. Buscamos los periodos dados de alta para ese ciclo
            var periodos = await _context.PeriodosInternos
                .Where(p => p.NivelEducativoId == nivelId && p.CicloEscolarId == cicloId && p.Activo)
                .OrderBy(p => p.Orden)
                .Select(p => new {
                    p.Id,
                    p.Nombre,
                    p.TrimestreSEP,
                    p.Orden,
                    p.AbiertoParaCaptura 
                })
                .ToListAsync();

            return Ok(new
            {
                Configuracion = config,
                Periodos = periodos
            });
        }

        // --- 2. GUARDAR REGLAS GENERALES ---
        // --- 2. GUARDAR REGLAS GENERALES ---
        [HttpPost("guardar-reglas")]
        public async Task<IActionResult> GuardarReglas([FromBody] GuardarReglasDto dto)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            var existente = await _context.ConfiguracionesAcademicas
                .FirstOrDefaultAsync(c => c.NivelEducativoId == dto.NivelEducativoId && c.Activo);

            if (existente != null)
            {
                existente.TipoPeriodoInterno = (TipoPeriodo)dto.TipoPeriodoInterno;
                existente.UsaDecimales = dto.UsaDecimales;
                existente.CalificacionAprobatoria = dto.CalificacionAprobatoria;
                existente.EscalaMinima = dto.EscalaMinima;
                existente.EscalaMaxima = dto.EscalaMaxima;
                existente.FUM = DateTime.Now;
                existente.Usuario = usuarioActual;

                _context.ConfiguracionesAcademicas.Update(existente);
            }
            else
            {
                // Si no existía, creamos la entidad completa aquí
                var nuevaRegla = new ConfiguracionAcademica
                {
                    NivelEducativoId = dto.NivelEducativoId,
                    TipoPeriodoInterno = (TipoPeriodo)dto.TipoPeriodoInterno,
                    UsaDecimales = dto.UsaDecimales,
                    CalificacionAprobatoria = dto.CalificacionAprobatoria,
                    EscalaMinima = dto.EscalaMinima,
                    EscalaMaxima = dto.EscalaMaxima,
                    Usuario = usuarioActual,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                _context.ConfiguracionesAcademicas.Add(nuevaRegla);
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Configuración guardada correctamente." });
        }

        // --- 3. AGREGAR UN PERIODO INTERNO (MES/BIMESTRE) ---
        [HttpPost("periodo")]
        public async Task<IActionResult> AgregarPeriodo([FromBody] PeriodoInterno periodo)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            periodo.Usuario = usuarioActual;
            periodo.FechaRegistro = DateTime.Now;
            periodo.Activo = true;

            _context.PeriodosInternos.Add(periodo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Periodo agregado correctamente." });
        }

        // --- 4. ELIMINAR UN PERIODO ---
        [HttpDelete("periodo/{id}")]
        public async Task<IActionResult> EliminarPeriodo(Guid id)
        {
            var periodo = await _context.PeriodosInternos.FindAsync(id);
            if (periodo == null) return NotFound("Periodo no encontrado.");

            // Validar que no haya calificaciones ya capturadas en este periodo
            bool tieneCalificaciones = await _context.CalificacionesInternas.AnyAsync(c => c.PeriodoInternoId == id && c.Activo);
            if (tieneCalificaciones) return BadRequest("No puedes eliminar este periodo porque ya tiene calificaciones capturadas.");

            periodo.Activo = false;
            periodo.FUM = DateTime.Now;
            _context.PeriodosInternos.Update(periodo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Periodo eliminado." });
        }

        // --- 5. ABRIR / CERRAR PERIODO PARA CAPTURA ---
        [HttpPut("periodo/{id}/toggle-captura")]
        public async Task<IActionResult> ToggleCapturaPeriodo(Guid id, [FromBody] bool abierto)
        {
            var periodo = await _context.PeriodosInternos.FindAsync(id);
            if (periodo == null) return NotFound("Periodo no encontrado.");

            periodo.AbiertoParaCaptura = abierto;
            periodo.FUM = DateTime.Now;
            periodo.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.PeriodosInternos.Update(periodo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Estado del periodo actualizado." });
        }
    }

    // Agrega esta clase al final de tu controlador
    public class GuardarReglasDto
    {
        public Guid NivelEducativoId { get; set; }
        public int TipoPeriodoInterno { get; set; }
        public bool UsaDecimales { get; set; }
        public decimal CalificacionAprobatoria { get; set; } // Renombrado
        public decimal EscalaMinima { get; set; } // Nuevo
        public decimal EscalaMaxima { get; set; } // Nuevo
    }
}