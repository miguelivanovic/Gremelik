using Gremelik.core.DTOs;
using Gremelik.core.Entities;
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

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
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
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Usuario no válido.");

            var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!checkPassword) return Unauthorized("Contraseña incorrecta.");

            // Generamos el token con Nombre y Roles
            var token = await GenerarToken(user);

            return Ok(new UserSessionDto
            {
                Token = token,
                Email = user.Email!,
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
