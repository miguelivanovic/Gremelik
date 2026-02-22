using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum EstatusCiclo
    {
        Actual = 1,
        Proximo = 2,
        Finalizado = 3
    }

    public class CicloEscolar
    {
        // VOLVEMOS A INT PARA NO ROMPER EL SISTEMA
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // EL NUEVO CAMPO DE ESTATUS
        public EstatusCiclo Estatus { get; set; } = EstatusCiclo.Proximo;

        // ESTO ARREGLA EL ERROR "No contiene definición para Actual"
        // Es un truco: Si alguien pregunta ".Actual", el sistema revisa si el estatus es 1.
        [NotMapped]
        public bool Actual
        {
            get { return Estatus == EstatusCiclo.Actual; }
            set { if (value) Estatus = EstatusCiclo.Actual; } // Setter dummy para compatibilidad
        }

        // Pertenece a la Escuela
        public Guid EscuelaId { get; set; }
        public Escuela? Escuela { get; set; }

        // CAMPOS DE AUDITORÍA (Los ponemos manual porque ya no heredamos de BaseEntity)
        public bool Activo { get; set; } = true;
        public string? Usuario { get; set; } // Lo ponemos opcional (?) para que no te de error en los news
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FUM { get; set; }
    }
}
