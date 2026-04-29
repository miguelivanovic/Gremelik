using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum TipoReglaBeca
    {
        Ninguna = 0,                 // Beca normal (Excelencia, Convenio, etc.)
        HermanoMayor = 1,            // Se aplica al hermano en el grado más alto
        HermanoMenor = 2,            // Se aplica al hermano en el grado más bajo
        ColegiaturaMasCara = 3,      // Se aplica al hermano con el plan de pago más alto
        ColegiaturaMasBarata = 4,    // Se aplica al hermano con el plan de pago más bajo
        TodosLosHermanos = 5         // Se aplica parejo a todos si tienen hermanos
    }

    public class Beca : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Puede ser por porcentaje o monto fijo
        [Column(TypeName = "decimal(18,2)")]
        public decimal Porcentaje { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoFijo { get; set; } = 0;

        // Reglas de Aplicación
        public bool AplicaEnInscripcion { get; set; } = false;
        public bool AplicaEnColegiatura { get; set; } = true;

        // --- NUEVA REGLA PARA HERMANOS ---
        public TipoReglaBeca ReglaHermano { get; set; } = TipoReglaBeca.Ninguna;

        // VINCULACIÓN CON EL CICLO
        public int CicloEscolarId { get; set; }

        public Guid EscuelaId { get; set; }
    }
}