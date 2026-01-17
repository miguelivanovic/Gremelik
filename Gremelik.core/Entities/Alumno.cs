
using System;
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Alumno : BaseEntity
    {
        [StringLength(50)]
        public string Nombre { get; set; }
        [StringLength(50)]
        public string PrimerApellido { get; set; }
        [StringLength(50)]
        public string SegundoApellido { get; set; }
        [StringLength(20)]
        public string Matricula { get; set; }
        [StringLength(18)]
        public string CURP { get; set; }
        [StringLength(10)]
        public string NIA { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public Guid EscuelaId { get; set; }
    }
}
