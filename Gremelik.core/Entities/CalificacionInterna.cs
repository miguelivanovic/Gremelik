using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class CalificacionInterna : BaseEntity
    {
        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        public int GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        public Guid MateriaId { get; set; }
        [ForeignKey("MateriaId")]
        public Materia? Materia { get; set; }

        public Guid PeriodoInternoId { get; set; }
        [ForeignKey("PeriodoInternoId")]
        public PeriodoInterno? Periodo { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Nota { get; set; }
    }
}
