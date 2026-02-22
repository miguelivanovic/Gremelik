using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class CuentaPorCobrar : BaseEntity
    {
        // ¿A QUIÉN LE COBRAMOS?
        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        public int CicloEscolarId { get; set; }

        // ¿QUÉ LE COBRAMOS?
        [Required]
        public string ConceptoNombre { get; set; } = string.Empty; // Ej: "Colegiatura Septiembre"

        // Referencia al concepto original (opcional, por si quieres estadísticas)
        public Guid? ConceptoPagoId { get; set; }

        // FECHAS
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; } // Null si no ha pagado

        // MONTOS (SNAPSHOT)
        // Guardamos los valores fijos para que si cambias el precio en el catálogo, 
        // NO afecte a lo que ya se le generó a este alumno.
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoBase { get; set; } // Precio de lista

        [Column(TypeName = "decimal(18,2)")]
        public decimal DescuentoBeca { get; set; } // Dinero descontado

        [Column(TypeName = "decimal(18,2)")]
        public decimal RecargosAcumulados { get; set; } // Por pagar tarde

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagado { get; set; } // Lo que ha abonado

        // ESTATUS
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, PAGADO, PARCIAL, CANCELADO

        // DATOS EXTRA
        public int NumeroDePago { get; set; } // Ej: Pago 1 de 10
        public Guid? BecaId { get; set; } // ¿Qué beca se aplicó?

        public Guid EscuelaId { get; set; }

        // --- NUEVO: ¿Este cargo específico se factura? ---
        public bool EsFacturable { get; set; } = false;

        // PROPIEDAD CALCULADA: ¿Cuánto falta por pagar?
        [NotMapped]
        public decimal SaldoPendiente => (MontoBase - DescuentoBeca + RecargosAcumulados) - TotalPagado;
    }
}
