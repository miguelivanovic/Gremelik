
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Tutor : BaseEntity
    {
        [StringLength(50)]
        public required string Nombre { get; set; }
        [StringLength(50)]
        public required string PrimerApellido { get; set; }
        [StringLength(50)]
        public string? SegundoApellido { get; set; }
        [StringLength(13)]
        public required string RFC { get; set; }
        [StringLength(50)]
        public string? RegimenFiscal { get; set; }
        [StringLength(5)]
        public string? CodigoPostal { get; set; }
    }
}
