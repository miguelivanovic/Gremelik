using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class Plantel
    {
        public Guid Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty; // Ej: "Campus Norte"

        // --- RELACIÓN CON LA CUENTA MAESTRA ---
        public Guid EscuelaId { get; set; }
        [ForeignKey("EscuelaId")]
        public Escuela? Escuela { get; set; }

        // --- DATOS DE UBICACIÓN (Vital para la SEP estatal) ---
        public string Calle { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;

        [Required]
        public string Estado { get; set; } = string.Empty; // Ej: "Durango", "Coahuila"

        // --- DATOS SEP (Específicos del Plantel) ---
        public string CCT { get; set; } = string.Empty; // Clave de Centro de Trabajo
        public string ZonaEscolar { get; set; } = string.Empty;
        public string JefaturaSector { get; set; } = string.Empty;

        // --- DATOS FISCALES (Facturación diferenciada) ---
        public string RazonSocial { get; set; } = string.Empty; // Ej: "Educación del Norte A.C."
        public string RFC { get; set; } = string.Empty;
        public string CodigoPostalFiscal { get; set; } = string.Empty; // A veces difiere del físico
        public string RegimenFiscal { get; set; } = string.Empty;

        public bool EsMatriz { get; set; } = false; // Para identificar el principal
        public bool Activo { get; set; } = true;
    }
}
