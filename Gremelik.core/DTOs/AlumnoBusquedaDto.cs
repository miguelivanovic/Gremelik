using Gremelik.core.Entities;

namespace Gremelik.core.DTOs
{
    public class AlumnoBusquedaDto
    {
        public Guid Id { get; set; }
        public string Matricula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string CURP { get; set; } = "";
        public string NIA { get; set; } = "";
        public EstatusAlumno Estatus { get; set; }

        // --- Datos de su Inscripción Actual ---
        public string Plantel { get; set; } = "";
        public string Nivel { get; set; } = "";
        public string Grado { get; set; } = "";
        public string Grupo { get; set; } = "";
    }
}
