using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class ExcepcionCaja : BaseEntity
    {
        public Guid PagoId { get; set; }
        [ForeignKey("PagoId")]
        public Pago? Pago { get; set; }

        public Guid CuentaPorCobrarId { get; set; } // Qué mensualidad se alteró

        // 2. AGREGA ESTE PUENTE PARA LA DEUDA (El que marca el error)
        [ForeignKey("CuentaPorCobrarId")]
        public CuentaPorCobrar? CuentaPorCobrar { get; set; }

        [Required]
        [StringLength(250)]
        public string Motivo { get; set; } = string.Empty; // Ej: "Autorizado por Dirección", "Falla en el sistema de bancos"

        // Para saber exactamente qué perdonó el cajero
        [Column(TypeName = "decimal(18,2)")]
        public decimal BecaRestauradaMonto { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RecargoPerdonadoMonto { get; set; } = 0;

        public Guid EscuelaId { get; set; }

        // El Usuario (Cajero) y la Fecha de esta acción ya los heredas de BaseEntity
    }
}