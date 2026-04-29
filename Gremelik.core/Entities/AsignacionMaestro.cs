using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class AsignacionMaestro : BaseEntity
    {
        // El ID del maestro (Identity ApplicationUser usa string por defecto)
        [Required]
        public string MaestroId { get; set; } = string.Empty;

        public Guid MateriaId { get; set; }
        [ForeignKey("MateriaId")]
        public Materia? Materia { get; set; }

        public int GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }
    }
}
