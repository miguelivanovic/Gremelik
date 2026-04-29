namespace Gremelik.core.DTOs
{
    public class UsuarioEditDto
    {
        public required string NombreCompleto { get; set; }
        public required string Email { get; set; }
        public string? Telefono { get; set; }
        public required string Rol { get; set; }
    }

    public class CambiarPasswordDto
    {
        public required string NuevaPassword { get; set; }
    }
}
