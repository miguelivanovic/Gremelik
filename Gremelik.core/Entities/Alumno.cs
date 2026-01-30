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
        public required string Nombre { get; set; }
        public required string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public required string Matricula { get; set; }
        public required string CURP { get; set; } 
        public required string NIA { get; set; }
        public DateTime FechaNacimiento { get; set; }

        // Relación con Escuela
        public Guid EscuelaId { get; set; }
        // Opcional: Propiedad de navegación si la usas
        // public Escuela Escuela { get; set; } 

        // Nuevo campo de Estatus
        public EstatusAlumno Estatus { get; set; } = EstatusAlumno.Activo;
    }
}