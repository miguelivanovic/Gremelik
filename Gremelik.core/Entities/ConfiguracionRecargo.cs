using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    // 1. ESTO ES LO QUE FALTA (EL ENUMERADOR)
    public enum TipoAplicacionRecargo
    {
        UnicaVez = 1,
        MensualAcumulativo = 2
    }

    public class ConfiguracionRecargo : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string NombreConcepto { get; set; } = "Cargo por Pago Tardío";

        public int DiasGracia { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoFijo { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Porcentaje { get; set; } = 0;

        // 2. ESTA ES LA PROPIEDAD QUE EL COMPILADOR NO ENCUENTRA
        public TipoAplicacionRecargo Tipo { get; set; } = TipoAplicacionRecargo.UnicaVez;

        public int CicloEscolarId { get; set; }
        public Guid EscuelaId { get; set; }

        // --- NUEVO: REGLAS FISCALES (IVA) ---
        public bool AplicaIva { get; set; } = false; // ¿Este recargo lleva IVA?
        public bool IvaIncluido { get; set; } = true; // Si es true: Los $50 ya traen el IVA. Si es false: Son $50 + IVA.
    }
}