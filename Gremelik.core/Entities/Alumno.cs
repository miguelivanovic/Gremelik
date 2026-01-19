using Gremelik.core.Entities;
using System;

namespace Gremelik.core.Entities
{
    public enum EstatusAlumno
    {
        Activo = 1,
        Baja = 2,
        Egresado = 3
    }

    public class Alumno : BaseEntity
    {
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Matricula { get; set; }
        public string CURP { get; set; } // <--- Esta es la que te faltaba
        public string NIA { get; set; }
        public DateTime FechaNacimiento { get; set; }

        // Relación con Escuela
        public Guid EscuelaId { get; set; } // <--- Esta también faltaba
        // Opcional: Propiedad de navegación si la usas
        // public Escuela Escuela { get; set; } 

        // Nuevo campo de Estatus
        public EstatusAlumno Estatus { get; set; } = EstatusAlumno.Activo;
    }
}