using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class CostoInscripcion : BaseEntity
    {
        // El precio está ligado a un Ciclo (Ej: 2025-2026)
        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        // Y puede ser específico por Nivel (Ej: Toda Primaria cuesta $5000)
        public Guid? NivelEducativoId { get; set; }
        [ForeignKey("NivelEducativoId")]
        public NivelEducativo? NivelEducativo { get; set; }

        // O específico por Grado (Ej: 1ro de Primaria cuesta $5500) - Opcional
        public int? GradoId { get; set; }
        [ForeignKey("GradoId")]
        public Grado? Grado { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; } // El precio base

        public string Concepto { get; set; } = "Inscripción"; // Para mostrar en el recibo
    }
}