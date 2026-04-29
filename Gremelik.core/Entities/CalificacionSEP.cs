using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class CalificacionSEP : BaseEntity
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

        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        // El Trimestre de la SEP (1, 2 o 3)
        public int Trimestre { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PromedioSugerido { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal NotaFinal { get; set; }

        // Si es TRUE, el maestro ya le dio "Cerrar" y no puede cambiar ni esta nota ni las internas
        public bool Confirmado { get; set; } = false;
    }
}
