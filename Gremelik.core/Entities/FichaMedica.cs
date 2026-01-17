
using System;
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class FichaMedica : BaseEntity
    {
        [StringLength(10)]
        public string TipoSangre { get; set; }
        [StringLength(200)]
        public string Alergias { get; set; }        
        [StringLength(100)]
        public string NombreContactoEmergencia { get; set; }
        [StringLength(20)]
        public string TelefonoContactoEmergencia { get; set; }
        public Guid AlumnoId { get; set; }
    }
}
