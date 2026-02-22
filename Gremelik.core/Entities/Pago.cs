using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum MetodoPago
    {
        Efectivo = 1,
        TarjetaDebito = 2,
        TarjetaCredito = 3,
        Transferencia = 4,
        Cheque = 5
    }

    public class Pago : BaseEntity
    {
        // DATOS DEL TICKET
        public int Folio { get; set; } // Número consecutivo legible (Ej: 1005)

        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagado { get; set; } // La suma total del ticket

        [Column(TypeName = "decimal(18,2)")]
        public decimal DineroRecibido { get; set; } // Con cuánto pagó (para calcular cambio)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cambio { get; set; }

        public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

        public string? Comentarios { get; set; } // Referencia bancaria, notas, etc.

        // RELACIÓN HIJOS
        public List<DetallePago> Detalles { get; set; } = new();

        // --- DATOS DE FACTURACIÓN ---
        public bool RequiereFactura { get; set; } = false;

        public Guid? TutorId { get; set; }
        [ForeignKey("TutorId")]
        public Tutor? Tutor { get; set; }

        public Guid EscuelaId { get; set; }
        public int CicloEscolarId { get; set; } // Para reportes de corte de caja por ciclo
    }
}