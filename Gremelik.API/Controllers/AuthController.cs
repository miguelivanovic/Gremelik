using Gremelik.core.DTOs;
using Gremelik.core.Entities;
using Gremelik.core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        // 1. DECLARAMOS LA VARIABLE DEL SERVICIO
        private readonly CurrentTenantService _tenantService;

        // 2. LO AGREGAMOS AL CONSTRUCTOR
        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            CurrentTenantService tenantService)
        {
            _userManager = userManager;
            _configuration = configuration;

            // 3. LO ASIGNAMOS
            _tenantService = tenantService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // 1. Crear el objeto usuario
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                NombreCompleto = model.NombreCompleto, // <-- Guardamos el nombre real
                // Si enviaron EscuelaId (lógica futura), se asignaría aquí
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // 2. Asignar el Rol solicitado (o SchoolAdmin por defecto)
            // Validamos que el rol sea uno permitido para evitar hackeos
            string rolAsignar = "SchoolAdmin";
            if (model.Rol == "User") rolAsignar = "User";
            // Nota: No permitimos crear "GlobalAdmin" desde aquí por seguridad.

            await _userManager.AddToRoleAsync(user, rolAsignar);

            return Ok(new { message = "Usuario registrado exitosamente con rol: " + rolAsignar });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // 1. Buscamos al usuario. 
            // TRUCO: Permitimos que inicie sesión con su Correo O con su UserName.
            var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Email);

            if (user == null) return Unauthorized("Usuario o contraseña incorrectos.");

            // 🛑 EL FILTRO MAESTRO 1: Revisar tu campo personalizado "Activo"
            if (!user.Activo)
            {
                return Unauthorized("Esta cuenta ha sido desactivada por la administración.");
            }

            // 🛑 EL FILTRO MAESTRO 2: Revisar el bloqueo oficial de Identity (Lockout)
            if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized("Esta cuenta se encuentra suspendida permanentemente.");
            }

            // ========================================================================
            // 🛑 EL FILTRO MAESTRO 3: Validar el Dominio vs El Usuario (Multi-Tenant)
            // ========================================================================
            var esGlobalAdmin = await _userManager.IsInRoleAsync(user, "GlobalAdmin");

            // Caso A: Intentan entrar al dominio principal (localhost:7106 sin subdominio)
            if (_tenantService.TenantId == null)
            {
                if (!esGlobalAdmin)
                {
                    return Unauthorized("Acceso denegado. Este portal es exclusivo para administración central. Por favor, ingresa desde la dirección web de tu escuela.");
                }
            }
            // Caso B: Intentan entrar a un subdominio de una escuela específica
            else
            {
                // Un GlobalAdmin puede entrar a cualquier escuela para dar soporte,
                // pero si NO es GlobalAdmin, su EscuelaId debe coincidir exactamente con el subdominio.
                if (!esGlobalAdmin && user.EscuelaId != _tenantService.TenantId)
                {
                    return Unauthorized("Acceso denegado. Este usuario no pertenece a esta escuela.");
                }
            }
            // ========================================================================

            // 2. Verificamos la contraseña (Ahora sí, sabiendo que el usuario es válido)
            var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!checkPassword) return Unauthorized("Usuario o contraseña incorrectos.");

            // 3. Generamos el token con Nombre y Roles
            var token = await GenerarToken(user);

            return Ok(new UserSessionDto
            {
                Token = token,
                Email = user.UserName!, // Regresamos el UserName como identificador principal
                Rol = "VerToken" // El rol real viaja encriptado en el token
            });
        }

        private async Task<string> GenerarToken(ApplicationUser user)
        {
            // 1. Claims básicos
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                // Aquí usamos el NombreCompleto. Si es nulo, usamos el Email como respaldo.
                new Claim(ClaimTypes.Name, user.NombreCompleto ?? user.Email!)
            };

            // 2. Agregar el ID de la Escuela si existe
            if (user.EscuelaId.HasValue)
            {
                claims.Add(new Claim("EscuelaId", user.EscuelaId.Value.ToString()));
            }

            // 3. Agregar los ROLES al token
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
