using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; } // Identificador Universal

        // El campo que pediste que fuera OBLIGATORIO
        public required string Usuario { get; set; }

        // --- ESTOS SON LOS CAMPOS QUE TE FALTAN Y CAUSAN EL ERROR ---
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FUM { get; set; } // Fecha Última Modificación
        public bool Activo { get; set; } = true;
    }
}
