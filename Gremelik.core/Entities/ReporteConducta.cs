using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public enum NivelGravedad
    {
        Leve = 1,
        Moderada = 2,
        Grave = 3,
        MuyGrave = 4 // Expulsión definitiva o suspensión grave
    }

    public enum EstatusReporte
    {
        Pendiente = 1,   // El maestro lo levantó, pero Coordinación no lo ha revisado
        EnProceso = 2,   // Se mandó citatorio a los padres / En investigación
        Cerrado = 3      // Ya se firmó el acuerdo o se aplicó la sanción
    }

    public class ReporteConducta : BaseEntity
    {
        // 1. EL TIEMPO Y ESPACIO
        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        public DateTime FechaIncidencia { get; set; } = DateTime.Now;

        // 2. LOS INVOLUCRADOS
        public Guid AlumnoId { get; set; }
        [ForeignKey("AlumnoId")]
        public Alumno? Alumno { get; set; }

        // Como los usuarios/maestros en ASP.NET Identity suelen usar un string como ID
        [Required]
        public string ReportadoPorId { get; set; } = string.Empty;

        // TRUCO DE ORO: Guardar el nombre de quien reporta aquí mismo nos ahorra 
        // tener que hacer un "Join" pesado con la tabla de Usuarios cada vez que veamos la lista.
        [Required]
        public string NombreReportador { get; set; } = string.Empty;

        // 3. EL SUCESO
        public NivelGravedad Gravedad { get; set; } = NivelGravedad.Leve;

        [Required]
        [MaxLength(100)]
        public string Titulo { get; set; } = string.Empty; // Ej. "Uso de celular en clase", "Falta de respeto"

        [Required]
        public string Descripcion { get; set; } = string.Empty; // "El alumno estaba jugando Free Fire durante el examen..."

        // 4. LA RESOLUCIÓN
        public string? AccionTomada { get; set; } // "Se le retiró el celular y se entregará a la salida a su tutor."

        public EstatusReporte Estatus { get; set; } = EstatusReporte.Pendiente;
    }
}