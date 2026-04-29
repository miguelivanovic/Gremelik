using System.ComponentModel.DataAnnotations.Schema;

namespace Gremelik.core.Entities
{
    public class PeriodoInterno : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty; // Ej. "Septiembre" o "Bimestre 1"

        public int CicloEscolarId { get; set; }
        [ForeignKey("CicloEscolarId")]
        public CicloEscolar? CicloEscolar { get; set; }

        public Guid NivelEducativoId { get; set; }
        [ForeignKey("NivelEducativoId")]
        public NivelEducativo? NivelEducativo { get; set; }

        // Aquí está la magia: amarra el mes al formato federal
        // 1 = Trimestre 1, 2 = Trimestre 2, 3 = Trimestre 3
        public int TrimestreSEP { get; set; }

        public int Orden { get; set; } // Para que en la tabla salgan ordenados: 1, 2, 3...

        public bool AbiertoParaCaptura { get; set; } = false; // Por defecto nacen cerrados
    }
}