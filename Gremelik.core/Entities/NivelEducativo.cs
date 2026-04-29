using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class NivelEducativo
    {
        public Guid Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty; // Ej: "Primaria", "Secundaria"

        public int Orden { get; set; } // 1, 2, 3 para ordenar en listas

        // Relación: Un nivel se ofrece en un Plantel específico
        public Guid PlantelId { get; set; }
        [ForeignKey("PlantelId")]
        public Plantel? Plantel { get; set; }

        // RVOE Específico para este nivel en este plantel
        public string RVOE { get; set; } = string.Empty;
        public DateTime? FechaRVOE { get; set; }

        // Valor exacto que exige el SAT para el complemento IEDU
        [StringLength(50)]
        public string NivelSAT { get; set; } = string.Empty;
    }
}
