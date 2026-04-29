using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Tutor : BaseEntity
    {
        [StringLength(50)]
        public required string Nombre { get; set; }

        [StringLength(50)]
        public required string PrimerApellido { get; set; }

        [StringLength(50)]
        public string? SegundoApellido { get; set; }

        // --- DATOS DE CONTACTO (NUEVOS) ---
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? CorreoElectronico { get; set; }

        [StringLength(20)]
        public string? TelefonoMovil { get; set; }

        [StringLength(150)]
        public string? DireccionFisica { get; set; } // Calle, Número, Colonia

        // --- DATOS FISCALES ---
        [StringLength(13)]
        public required string RFC { get; set; }

        [StringLength(50)]
        public string? RegimenFiscal { get; set; }

        [StringLength(5)]
        public string? CodigoPostal { get; set; }

        [StringLength(4)]
        public string UsoCFDI { get; set; } = "D10"; // Valor por defecto para colegiaturas
    }
}