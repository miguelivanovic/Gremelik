
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Escuela : BaseEntity
    {
        [StringLength(100)]
        public string Nombre { get; set; }
        [StringLength(100)]
        public string RazonSocial { get; set; }
        [StringLength(20)]
        public string CCT { get; set; }
        [StringLength(20)]
        public string RVOE { get; set; }
    }
}
