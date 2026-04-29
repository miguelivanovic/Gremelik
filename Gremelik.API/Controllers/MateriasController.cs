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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador")]
    public class MateriasController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public MateriasController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("plantel/{plantelId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMaterias(Guid plantelId)
        {
            var materias = await _context.Materias
                .Include(m => m.Grado)
                .Include(m => m.Grupo)
                .Where(m => m.PlantelId == plantelId && m.Activo)
                .OrderBy(m => m.CampoFormativo).ThenBy(m => m.GradoId).ThenBy(m => m.Nombre)
                .Select(m => new {
                    m.Id,
                    ClaveOficial = m.ClaveOficial, // <--- NUEVO
                    m.Nombre,
                    CampoFormativo = m.CampoFormativo ?? "Sin Especificar", // <--- NUEVO
                    m.GradoId,
                    GradoNombre = m.Grado != null ? m.Grado.Nombre : "Aplica a todos los grados",
                    m.GrupoId,
                    GrupoNombre = m.Grupo != null ? m.Grupo.Nombre : "Aplica a todos los grupos"
                })
                .ToListAsync();

            return Ok(materias);
        }

        [HttpPost]
        public async Task<IActionResult> CrearMateria([FromBody] Materia materia)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            materia.Usuario = usuarioActual;
            materia.FechaRegistro = DateTime.Now;
            materia.Activo = true;

            // Limpieza de datos: Si mandaron 0 desde el select del frontend, lo convertimos a null
            if (materia.GradoId == 0) materia.GradoId = null;
            if (materia.GrupoId == 0) materia.GrupoId = null;

            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Materia creada exitosamente" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMateria(Guid id)
        {
            var materia = await _context.Materias.FindAsync(id);
            if (materia == null) return NotFound("Materia no encontrada");

            // Borrado lógico
            materia.Activo = false;
            materia.FUM = DateTime.Now;

            _context.Materias.Update(materia);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Materia eliminada" });
        }
    }
}