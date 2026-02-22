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
    public class GruposController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public GruposController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // GET: api/Grupos/grado/5/ciclo/10
        [HttpGet("grado/{gradoId}/ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<Grupo>>> GetGruposPorGrado(int gradoId, int cicloId)
        {
            return await _context.Grupos
                .Where(g => g.GradoId == gradoId && g.CicloEscolarId == cicloId)
                .OrderBy(g => g.Nombre)
                .ToListAsync();
        }

        // GET: api/Grupos/ciclo/10
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<Grupo>>> GetGruposPorCiclo(int cicloId)
        {
            return await _context.Grupos
                .Include(g => g.Grado).ThenInclude(gr => gr.NivelEducativo)
                .Where(g => g.CicloEscolarId == cicloId)
                .OrderBy(g => g.Grado!.NivelEducativoId)
                .ThenBy(g => g.Grado!.Numero)
                .ThenBy(g => g.Nombre)
                .ToListAsync();
        }

        // POST: api/Grupos (CREAR)
        [HttpPost]
        public async Task<ActionResult<Grupo>> PostGrupo(Grupo grupo)
        {
            // 1. Validaciones Básicas
            if (string.IsNullOrEmpty(grupo.Nombre)) return BadRequest("El nombre es obligatorio");
            if (grupo.CupoMaximo <= 0) return BadRequest("El cupo debe ser mayor a 0");

            // 2. VALIDACIÓN DE DUPLICADOS (NUEVO)
            // Verificamos si ya existe un grupo con el mismo Nombre, en el mismo Grado y Ciclo
            bool existe = await _context.Grupos.AnyAsync(g =>
                g.CicloEscolarId == grupo.CicloEscolarId &&
                g.GradoId == grupo.GradoId &&
                g.Nombre == grupo.Nombre
            );

            if (existe)
            {
                return BadRequest($"Ya existe un grupo '{grupo.Nombre}' en este grado. Por favor usa otro nombre.");
            }

            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";
            grupo.Usuario = usuarioActual;
            grupo.FechaRegistro = DateTime.Now;
            grupo.Activo = true;

            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGruposPorGrado", new { gradoId = grupo.GradoId, cicloId = grupo.CicloEscolarId }, grupo);
        }

        // PUT: api/Grupos/5 (MODIFICAR)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGrupo(int id, Grupo grupo)
        {
            if (id != grupo.Id) return BadRequest("ID no coincide");

            var existente = await _context.Grupos.FindAsync(id);
            if (existente == null) return NotFound();

            // 1. VALIDACIÓN DE DUPLICADOS AL EDITAR (NUEVO)
            // Verificamos si el NUEVO nombre ya existe en ese grado (excluyendo al grupo actual)
            bool duplicado = await _context.Grupos.AnyAsync(g =>
                g.CicloEscolarId == existente.CicloEscolarId && // Mismo ciclo
                g.GradoId == existente.GradoId &&               // Mismo grado
                g.Nombre == grupo.Nombre &&                     // Mismo nombre nuevo
                g.Id != id                                      // ¡IMPORTANTE! Que no sea yo mismo
            );

            if (duplicado)
            {
                return BadRequest($"Ya existe otro grupo llamado '{grupo.Nombre}' en este grado.");
            }

            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            // Actualizamos datos
            existente.Nombre = grupo.Nombre;
            existente.Turno = grupo.Turno;
            existente.CupoMaximo = grupo.CupoMaximo;

            // Auditoría
            existente.Usuario = usuarioActual;
            existente.FUM = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Grupos/5 (ELIMINAR)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrupo(int id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null) return NotFound();

            // REGLA DE ORO: No eliminar si ya hay alumnos inscritos
            bool tieneAlumnos = await _context.Inscripciones.AnyAsync(i => i.GrupoId == id);
            if (tieneAlumnos)
            {
                return BadRequest("No se puede eliminar el grupo porque ya tiene alumnos inscritos. Debes darlos de baja o moverlos primero.");
            }

            _context.Grupos.Remove(grupo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
