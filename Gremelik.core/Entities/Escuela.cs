
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Escuela : BaseEntity
    {
        [StringLength(100)]
        public required string Nombre { get; set; }
        [StringLength(100)]
        public required string RazonSocial { get; set; }
        [StringLength(20)]
        public required string CCT { get; set; }
        [StringLength(20)]
        public string? RVOE { get; set; }
    }
}
