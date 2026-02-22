using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class DetallePago : BaseEntity
    {
        public Guid PagoId { get; set; }
        [ForeignKey("PagoId")]
        public Pago? Pago { get; set; }

        // ¿QUÉ DEUDA ESTAMOS MATANDO?
        public Guid CuentaPorCobrarId { get; set; }
        [ForeignKey("CuentaPorCobrarId")]
        public CuentaPorCobrar? CuentaPorCobrar { get; set; }

        // ¿CUÁNTO ABONAMOS A ESTE CONCEPTO ESPECÍFICO?
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoAbonado { get; set; }

        // Guardamos el nombre aquí también por si borran el concepto original, el ticket quede intacto
        public string ConceptoNombreSnapshot { get; set; } = string.Empty;
    }
}