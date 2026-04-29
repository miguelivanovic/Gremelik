using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class BitacoraAsistencia : BaseEntity
    {
        public int GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        public Guid MateriaId { get; set; }
        [ForeignKey("MateriaId")]
        public Materia? Materia { get; set; }

        public DateTime Fecha { get; set; }
        public string MaestroId { get; set; } = string.Empty;
    }
}
