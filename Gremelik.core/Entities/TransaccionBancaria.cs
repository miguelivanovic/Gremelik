using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum EstatusTransaccion
    {
        Aplicada = 1,  // Se cobró automáticamente
        Huerfana = 2,  // No se encontró la matrícula (Espera conciliación manual)
        Duplicada = 3, // El archivo se subió dos veces
        Error = 4      // Falló por validación interna
    }

    public class TransaccionBancaria : BaseEntity
    {
        public Guid EscuelaId { get; set; }
        [ForeignKey("EscuelaId")]
        public Escuela? Escuela { get; set; }

        [Required]
        [StringLength(50)]
        public string Banco { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ReferenciaBancaria { get; set; } = string.Empty; // Lo que tecleó el papá

        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }

        [Required]
        [StringLength(100)]
        public string ClaveRastreo { get; set; } = string.Empty; // El candado anti-duplicados

        public EstatusTransaccion Estatus { get; set; } = EstatusTransaccion.Huerfana;

        // Si se logró asociar a un alumno (ya sea automático o en la pantalla manual)
        public Guid? AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        // Si este renglón generó un recibo de pago, lo enlazamos para rastrearlo
        public Guid? PagoGeneradoId { get; set; }
        [ForeignKey("PagoGeneradoId")]
        public Pago? PagoGenerado { get; set; }
    }
}