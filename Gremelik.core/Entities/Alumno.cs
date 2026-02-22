using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Required]
        public required string Nombre { get; set; }

        [Required]
        public required string PrimerApellido { get; set; }

        public string? SegundoApellido { get; set; }

        [Required]
        public required string Matricula { get; set; }

        [Required]
        [StringLength(18)]
        public required string CURP { get; set; }

        public required string NIA { get; set; }

        public DateTime FechaNacimiento { get; set; }

        // --- CORRECCIÓN IMPORTANTE: USAMOS GUID ---
        public Guid EscuelaId { get; set; }

        [ForeignKey("EscuelaId")]
        public Escuela? Escuela { get; set; }

        public EstatusAlumno Estatus { get; set; } = EstatusAlumno.Activo;
    }
}