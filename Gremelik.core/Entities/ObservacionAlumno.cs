using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class ObservacionAlumno : BaseEntity
    {
        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        public int GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        // --- NUEVO CAMPO ---
        public Guid MateriaId { get; set; }
        [ForeignKey("MateriaId")]
        public Materia? Materia { get; set; }
        // -------------------

        public string Notas { get; set; } = string.Empty; // Aquí el maestro vaciará su observación general
    }
}
