using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Escuela : BaseEntity
    {
        // --- IDENTIDAD ---
        [StringLength(100)]
        public required string Nombre { get; set; } // Nombre Comercial (Ej: "Instituto Gremelik")

        [StringLength(50)]
        [RegularExpression(@"^[a-z0-9]+$", ErrorMessage = "Solo minúsculas y números")]
        public required string Subdominio { get; set; } // La llave técnica (Ej: "benito")

        // --- BRANDING / DISEÑO WEB (Nuevos) ---

        [StringLength(255)]
        public string? LogoUrl { get; set; } // URL de la imagen del logo

        [StringLength(255)]
        public string? FondoLoginUrl { get; set; } // Imagen de fondo para su pantalla de Login

        [StringLength(7)]
        public string ColorPrimario { get; set; } = "#0d6efd"; // Hexadecimal (Default: Azul Bootstrap)

        [StringLength(7)]
        public string ColorSecundario { get; set; } = "#6c757d"; // Hexadecimal (Default: Gris)

        // --- EXTRAS DE MARKETING ---
        [StringLength(200)]
        public string? Slogan { get; set; } // Ej: "Educando para el futuro"

        [StringLength(255)]
        public string? SitioWeb { get; set; } // Si tienen una página web externa (.com)
    }
}