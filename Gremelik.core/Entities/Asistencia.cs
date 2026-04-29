using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum EstatusAsistencia
    {
        Presente = 1,
        Falta = 2,
        Retardo = 3,
        Justificado = 4
    }

    public class Asistencia : BaseEntity
    {
        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        public int GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }


        // --- NUEVO CAMPO: CICLO ESCOLAR (Con el ? para aceptar nulos en datos viejos) ---
        public int? CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }
        // ----------------------------------

        // --- NUEVO CAMPO: MATERIA ---
        public Guid MateriaId { get; set; }
        [ForeignKey("MateriaId")]
        public Materia? Materia { get; set; }
        // ----------------------------


        public DateTime Fecha { get; set; }
        public EstatusAsistencia Estatus { get; set; } = EstatusAsistencia.Presente;
        public string? Comentarios { get; set; }
    }
}
