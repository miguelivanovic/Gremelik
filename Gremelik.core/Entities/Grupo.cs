using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class Grupo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Nombre { get; set; } = string.Empty; // Ej: "A", "B", "Unico"

        [Required]
        public string Turno { get; set; } = "Matutino"; // Matutino, Vespertino, Nocturno

        public int CupoMaximo { get; set; } = 40;

        // --- RELACIONES (La Conexión Total) ---

        // 1. ¿De qué grado es? (Ej: 1er Grado)
        public int GradoId { get; set; }
        [ForeignKey("GradoId")]
        public Grado? Grado { get; set; }

        // 2. ¿De qué ciclo? (Ej: 2025-2026)
        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        // 3. (Opcional) Tutor del grupo (Maestro responsable)
        public string? MaestroTutorId { get; set; }
        [ForeignKey("MaestroTutorId")]
        public ApplicationUser? MaestroTutor { get; set; }

        // --- CAMPOS DE AUDITORÍA (LOS QUE FALTABAN) ---
        public bool Activo { get; set; } = true;

        // Agregamos estos 3 manualmente ya que no hereda de BaseEntity
        public required string Usuario { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FUM { get; set; } // Fecha Última Modificación
    }
}