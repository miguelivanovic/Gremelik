using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class Grado
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty; // Ej: "1er Grado", "Semestre 1"

        [Required]
        public int Numero { get; set; } // 1, 2, 3 (Para ordenar numéricamente)

        // Relación: Un Grado pertenece a un Nivel (Ej: 1ro pertenece a Primaria)
        public Guid NivelEducativoId { get; set; } // OJO: Si cambiaste a Guid en Nivel, esto debe ser Guid
        [ForeignKey("NivelEducativoId")]
        public NivelEducativo? NivelEducativo { get; set; }

        public bool Activo { get; set; } = true;
    }
}
