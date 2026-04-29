using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gremelik.core.DTOs
{
    public class AlumnoGrupoDto
    {
        public Guid InscripcionId { get; set; }
        public Guid AlumnoId { get; set; }
        public string Matricula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public int? GrupoId { get; set; } // Si es null, está "Sin Asignar"
        public string GrupoNombre { get; set; } = "";
    }

    public class MoverAlumnoGrupoDto
    {
        public Guid InscripcionId { get; set; }
        public int? NuevoGrupoId { get; set; } // Puede ser null si lo sacan del salón
    }

    public class AsignacionRandomDto
    {
        public int CicloId { get; set; }
        public int GradoId { get; set; }
    }
}
