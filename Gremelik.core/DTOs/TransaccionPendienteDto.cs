using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gremelik.core.DTOs
{
    public class TransaccionPendienteDto
    {
        public Guid Id { get; set; }
        public DateTime FechaPago { get; set; }
        public string Banco { get; set; } = "";
        public string ReferenciaBancaria { get; set; } = "";
        public decimal Monto { get; set; }
        public string ClaveRastreo { get; set; } = "";

        // Datos del Alumno Sugerido (si el motor encontró la matrícula pero el monto no cuadró)
        public Guid? AlumnoSugeridoId { get; set; }
        public string MatriculaSugerida { get; set; } = "";
        public string NombreSugerido { get; set; } = "";
    }
}
