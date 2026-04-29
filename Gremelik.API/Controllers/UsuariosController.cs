using Gremelik.core.DTOs;
using Gremelik.core.Entities;
using Gremelik.core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // <--- NUEVO: Para manejar roles
        private readonly CurrentTenantService _tenantService;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            CurrentTenantService tenantService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantService = tenantService;
        }

        // NUEVO ENDPOINT: Para llenar el combo de roles en la pantalla
        [HttpGet("roles")]
        public async Task<ActionResult<List<string>>> GetRolesDisponibles()
        {
            // Traemos todos los roles excepto GlobalAdmin (por seguridad, para que no se asigne por error)
            var roles = await _roleManager.Roles
                .Where(r => r.Name != "GlobalAdmin")
                .Select(r => r.Name)
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var query = _userManager.Users.AsQueryable();
            if (_tenantService.TenantId != null) query = query.Where(u => u.EscuelaId == _tenantService.TenantId);

            var usuarios = await query.ToListAsync();
            var listaResultado = new List<object>();

            foreach (var u in usuarios)
            {
                var rolesDelUsuario = await _userManager.GetRolesAsync(u);
                listaResultado.Add(new
                {
                    u.Id,
                    u.UserName, // Agregamos el nombre de usuario
                    u.NombreCompleto,
                    u.Email,
                    Telefono = u.PhoneNumber, // Usamos el campo nativo de Identity
                    Rol = rolesDelUsuario.FirstOrDefault() ?? "Sin Rol",
                    Activo = u.Activo
                });
            }
            return Ok(listaResultado);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName, // Ahora usamos el UserName independiente
                Email = model.Email,
                PhoneNumber = model.Telefono, // Guardamos el teléfono
                NombreCompleto = model.NombreCompleto,
                EscuelaId = _tenantService.TenantId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            string rolAsignar = string.IsNullOrEmpty(model.Rol) ? "User" : model.Rol;
            if (rolAsignar == "GlobalAdmin") return Forbid("No puedes crear Super Admins.");

            await _userManager.AddToRoleAsync(user, rolAsignar);
            return Ok(new { message = "Usuario creado exitosamente" });
        }

        // PUT: api/Usuarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(string id, [FromBody] UsuarioEditDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.EscuelaId != _tenantService.TenantId) return NotFound("Usuario no encontrado.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("GlobalAdmin")) return Forbid("No puedes editar a un Super Admin.");

            // 1. Actualizamos datos de contacto (El UserName NO se toca)
            user.NombreCompleto = model.NombreCompleto;
            user.Email = model.Email;
            user.PhoneNumber = model.Telefono;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return BadRequest("Error al actualizar los datos.");

            // 2. Actualizamos el Rol si cambió
            var rolActual = currentRoles.FirstOrDefault();
            if (rolActual != model.Rol)
            {
                if (model.Rol == "GlobalAdmin") return Forbid("No puedes asignar el rol de Super Admin.");
                if (rolActual != null) await _userManager.RemoveFromRoleAsync(user, rolActual);
                if (!string.IsNullOrEmpty(model.Rol)) await _userManager.AddToRoleAsync(user, model.Rol);
            }

            return Ok(new { message = "Usuario actualizado" });
        }

        // --- NUEVO: Endpoint para resetear contraseña ---
        [HttpPut("{id}/password")]
        public async Task<IActionResult> CambiarPassword(string id, [FromBody] CambiarPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.EscuelaId != _tenantService.TenantId) return NotFound("Usuario no encontrado.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("GlobalAdmin")) return Forbid("No puedes cambiar la contraseña de un Super Admin.");

            // Generamos un token maestro (como administradores) para forzar el cambio sin pedir la clave anterior
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NuevaPassword);

            if (result.Succeeded) return Ok();
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.EscuelaId != _tenantService.TenantId) return NotFound();

            // 1. Bloqueo oficial de Identity (Seguridad)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            // 2. Tu campo personalizado (Para reportes y filtros)
            user.Activo = false;

            // 3. Guardar cambios en el usuario
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Ok();
            return BadRequest("No se pudo inactivar.");
        }
    }


}
