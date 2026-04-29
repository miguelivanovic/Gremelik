using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class Materia : BaseEntity
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ClaveOficial { get; set; } // Ej: MAT01, LENG-3

        // --- NUEVO CAMPO SEP ---
        [StringLength(100)]
        public string CampoFormativo { get; set; } = "Sin Especificar";

        public Guid PlantelId { get; set; }
        [ForeignKey("PlantelId")]
        public Plantel? Plantel { get; set; }

        // Si tiene GradoId y NO tiene GrupoId = Aplica a todos los grupos de ese grado
        public int? GradoId { get; set; }
        [ForeignKey("GradoId")]
        public Grado? Grado { get; set; }

        // Si tiene GrupoId = Es exclusiva para ese salón en particular
        public int? GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }
    }
}
