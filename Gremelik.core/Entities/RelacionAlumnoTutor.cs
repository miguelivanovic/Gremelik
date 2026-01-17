
using System;
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class RelacionAlumnoTutor : BaseEntity
    {
        public Guid AlumnoId { get; set; }
        public Guid TutorId { get; set; }
        [StringLength(50)]
        public string Parentesco { get; set; }
    }
}
