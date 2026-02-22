using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum TipoDescuento
    {
        FechaLimite = 1,    // "Paga antes del 1 de Febrero"
        NuevoIngreso = 2,   // "Solo para nuevos"
        Reingreso = 3,      // "Solo para alumnos que vuelven"
        Hermanos = 4,       // "Si tiene hermanos inscritos"
        Manual = 5          // "Descuento especial del Director"
    }

    public class ReglaDescuento : BaseEntity
    {
        [Required]
        public required string Nombre { get; set; } // Ej: "Pronto Pago Febrero"

        public TipoDescuento Tipo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Porcentaje { get; set; } = 0; // Ej: 10%

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoFijo { get; set; } = 0; // Ej: $500 pesos menos

        // Para descuentos por fecha
        public DateTime? FechaInicioValidez { get; set; }
        public DateTime? FechaFinValidez { get; set; }

        // Relación con el Ciclo (Las promos son por año)
        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        // Relación con la Escuela (Tenant)
        public Guid EscuelaId { get; set; }
    }
}