using Gremelik.core.DTOs;
using Gremelik.core.Entities;
using Gremelik.core.Services; // Para CurrentTenantService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")] // Solo admins pueden gestionar usuarios
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CurrentTenantService _tenantService;

        public UsuariosController(UserManager<ApplicationUser> userManager, CurrentTenantService tenantService)
        {
            _userManager = userManager;
            _tenantService = tenantService;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<List<ApplicationUser>>> GetUsuarios()
        {
            // 1. Si es GlobalAdmin, ve todos. Si es SchoolAdmin, solo los de su escuela.
            var query = _userManager.Users.AsQueryable();

            if (_tenantService.TenantId != null)
            {
                // Filtramos por la escuela actual
                query = query.Where(u => u.EscuelaId == _tenantService.TenantId);
            }
            else
            {
                // Si no hay TenantId (es GlobalAdmin viendo todo), podrías querer filtrar o no.
                // Por seguridad, si no hay tenant, mejor no mostramos nada a menos que sea explícito.
                // Pero como tú eres el GlobalAdmin "sin escuela", verás a todos.
            }

            // Ojo: No devolvemos el Hash del password por seguridad
            var usuarios = await query.Select(u => new
            {
                u.Id,
                u.NombreCompleto,
                u.Email,
                u.EscuelaId,
                Rol = "Cargando..." // El rol es más complejo de sacar en una sola consulta, lo dejamos pendiente visualmente
            }).ToListAsync();

            return Ok(usuarios);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] RegisterDto model)
        {
            // 1. Validar que quien crea tenga permiso
            // Si soy SchoolAdmin, el nuevo usuario A FUERZA debe ser de mi escuela
            if (_tenantService.TenantId != null)
            {
                // Asignamos la escuela automáticamente
                // Convertimos el ID (Guid?) a string o lo usamos directo según tu DTO, 
                // pero aquí lo asignaremos directo a la entidad.
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                NombreCompleto = model.NombreCompleto,
                // ASIGNACIÓN AUTOMÁTICA DE ESCUELA:
                EscuelaId = _tenantService.TenantId
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // 2. Asignar Rol
            // Si el DTO trae rol, lo usamos. Si no, por defecto "User" (Maestro/Alumno)
            string rolAsignar = string.IsNullOrEmpty(model.Rol) ? "User" : model.Rol;

            // SEGURIDAD: Un SchoolAdmin NO puede crear un GlobalAdmin
            if (rolAsignar == "GlobalAdmin") return Forbid("No puedes crear Super Admins.");

            await _userManager.AddToRoleAsync(user, rolAsignar);

            return Ok(new { message = "Usuario creado exitosamente" });
        }
    }
}
