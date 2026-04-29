using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class PlanPago : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public int NumeroPagos { get; set; } = 10;
        public int DiaLimitePago { get; set; } = 10;

        // --- NUEVO CAMPO: Mes en el que inicia el cobro (1 = Enero, 12 = Diciembre) ---
        public int MesInicioCobro { get; set; } = 9; // Default: Septiembre

        [StringLength(50)]
        public string MesesDobleCobro { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal RecargoMonto { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RecargoPorcentaje { get; set; } = 0;

        public int CicloEscolarId { get; set; }

        public Guid ConceptoPagoId { get; set; }
        [ForeignKey("ConceptoPagoId")]
        public ConceptoPago? ConceptoRelacionado { get; set; }

        public Guid EscuelaId { get; set; }
    }
}