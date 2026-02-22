using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class Inscripcion : BaseEntity
    {
        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        public int? GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        // --- NUEVO CAMPO: GRADO ---
        // Esto permite saber el grado aunque no tenga grupo asignado aún
        public int? GradoId { get; set; }
        [ForeignKey("GradoId")]
        public Grado? Grado { get; set; }
        // --------------------------

        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        public Guid PlantelId { get; set; }
        [ForeignKey("PlantelId")]
        public Plantel? Plantel { get; set; }

        // Datos Financieros
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoBase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoDescuento { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoFinal { get; set; }

        public Guid? ReglaDescuentoId { get; set; }
        [ForeignKey("ReglaDescuentoId")]
        public ReglaDescuento? ReglaDescuento { get; set; }

        public string? MotivoDescuentoManual { get; set; }
        public bool EsNuevoIngreso { get; set; } = false;
        public string Estado { get; set; } = "Inscrito";
    }
}