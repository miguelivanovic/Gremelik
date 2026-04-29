using Gremelik.core.Entities;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador")]
    public class AsignacionesController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CurrentTenantService _tenantService;

        public AsignacionesController(GremelikDbContext context, UserManager<ApplicationUser> userManager, CurrentTenantService tenantService)
        {
            _context = context;
            _userManager = userManager;
            _tenantService = tenantService;
        }

        // 1. OBTENER LISTA DE MAESTROS DE ESTA ESCUELA
        [HttpGet("maestros")]
        public async Task<ActionResult> GetMaestros()
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada.");

            // Buscamos a todos los usuarios que tienen el rol "Maestro"
            var todosLosMaestros = await _userManager.GetUsersInRoleAsync("Maestro");

            // Filtramos para que solo salgan los de la escuela actual
            var maestrosEscuela = todosLosMaestros
                .Where(m => m.EscuelaId == _tenantService.TenantId.Value && m.Activo)
                .Select(m => new { m.Id, m.NombreCompleto, m.Email })
                .OrderBy(m => m.NombreCompleto)
                .ToList();

            return Ok(maestrosEscuela);
        }

        // 2. OBTENER ASIGNACIONES DEL CICLO
        [HttpGet("ciclo/{cicloId}")]
        public async Task<ActionResult> GetAsignaciones(int cicloId)
        {
            var asignaciones = await _context.AsignacionesMaestros
                .Include(a => a.Materia)
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .Where(a => a.CicloEscolarId == cicloId && a.Activo)
                .ToListAsync();

            // Como MaestroId es un string (Identity), cruzamos los datos manualmente para sacar su nombre
            var listaResultado = new List<object>();
            foreach (var a in asignaciones)
            {
                var maestro = await _userManager.FindByIdAsync(a.MaestroId);
                listaResultado.Add(new
                {
                    a.Id,
                    MaestroNombre = maestro?.NombreCompleto ?? "Maestro Desconocido",
                    MateriaNombre = a.Materia?.Nombre ?? "Sin Materia",
                    GrupoNombre = $"{a.Grupo?.Grado?.Nombre} {a.Grupo?.Nombre}",
                    Turno = a.Grupo?.Turno
                });
            }

            return Ok(listaResultado.OrderBy(x => x.GetType().GetProperty("GrupoNombre")?.GetValue(x, null)));
        }

        // 3. CREAR ASIGNACIÓN
        [HttpPost]
        public async Task<IActionResult> CrearAsignacion([FromBody] AsignacionMaestro asignacion)
        {
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            // Validar que no exista la misma materia para ese mismo grupo (opcional, pero buena práctica)
            bool yaExiste = await _context.AsignacionesMaestros
                .AnyAsync(a => a.GrupoId == asignacion.GrupoId &&
                               a.MateriaId == asignacion.MateriaId &&
                               a.CicloEscolarId == asignacion.CicloEscolarId &&
                               a.Activo);

            if (yaExiste) return BadRequest("Ese grupo ya tiene un maestro asignado para esa materia en este ciclo.");

            asignacion.Usuario = usuarioActual;
            asignacion.FechaRegistro = DateTime.Now;
            asignacion.Activo = true;

            _context.AsignacionesMaestros.Add(asignacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Asignación creada con éxito." });
        }

        // 4. ELIMINAR ASIGNACIÓN
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAsignacion(Guid id)
        {
            var asignacion = await _context.AsignacionesMaestros.FindAsync(id);
            if (asignacion == null) return NotFound("Asignación no encontrada.");

            asignacion.Activo = false;
            asignacion.FUM = DateTime.Now;

            _context.AsignacionesMaestros.Update(asignacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Asignación eliminada." });
        }
    }
}
