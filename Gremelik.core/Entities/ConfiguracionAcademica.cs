using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum TipoPeriodo
    {
        Mensual = 1,
        Bimestral = 2,
        Trimestral = 3
    }

    public class ConfiguracionAcademica : BaseEntity
    {
        public Guid NivelEducativoId { get; set; }
        [ForeignKey("NivelEducativoId")]
        public NivelEducativo? NivelEducativo { get; set; }

        public TipoPeriodo TipoPeriodoInterno { get; set; } = TipoPeriodo.Mensual;

        // true = acepta 8.5 | false = redondea a 9
        public bool UsaDecimales { get; set; } = true;

        // --- LOS 3 CAMPOS NUEVOS DE ESCALA ---
        [Column(TypeName = "decimal(5,2)")]
        public decimal CalificacionAprobatoria { get; set; } = 6.0m; // Ej. 6 o 70

        [Column(TypeName = "decimal(5,2)")]
        public decimal EscalaMinima { get; set; } = 5.0m; // Ej. 5 o 0

        [Column(TypeName = "decimal(5,2)")]
        public decimal EscalaMaxima { get; set; } = 10.0m; // Ej. 10 o 100
    }
}
