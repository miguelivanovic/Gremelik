using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

    }


    public class RegisterDto
    {
        public required string NombreCompleto { get; set; }
        public required string UserName { get; set; } // El usuario para iniciar sesión
        public required string Email { get; set; }
        public string? Telefono { get; set; }
        public required string Password { get; set; }
        public required string Rol { get; set; }

        // Opcional: El nombre de la escuela si es un registro nuevo
        public string NombreEscuela { get; set; } = string.Empty;
        public string Subdominio { get; set; } = string.Empty;
    }

    // Lo que le devolvemos al frontend al loguearse con éxito
    public class UserSessionDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}