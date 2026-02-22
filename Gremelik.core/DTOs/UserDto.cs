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
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        public string Rol { get; set; } = "SchoolAdmin"; // Por defecto será Director

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