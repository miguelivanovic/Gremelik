
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Tutor : BaseEntity
    {
        [StringLength(50)]
        public string Nombre { get; set; }
        [StringLength(50)]
        public string PrimerApellido { get; set; }
        [StringLength(50)]
        public string SegundoApellido { get; set; }
        [StringLength(13)]
        public string RFC { get; set; }
        [StringLength(50)]
        public string RegimenFiscal { get; set; }
        [StringLength(5)]
        public string CodigoPostal { get; set; }
    }
}
