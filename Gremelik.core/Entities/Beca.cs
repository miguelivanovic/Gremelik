using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class Beca : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Puede ser por porcentaje o monto fijo
        [Column(TypeName = "decimal(18,2)")]
        public decimal Porcentaje { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoFijo { get; set; } = 0;

        // Reglas de Aplicación
        public bool AplicaEnInscripcion { get; set; } = false;
        public bool AplicaEnColegiatura { get; set; } = true;

        // VINCULACIÓN CON EL CICLO
        public int CicloEscolarId { get; set; }

        public Guid EscuelaId { get; set; }
    }
}
