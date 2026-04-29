using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Gremelik.core.Entities
{
    public class Factura : BaseEntity
    {
        public Guid PagoId { get; set; }
        [ForeignKey("PagoId")]
        public Pago? Pago { get; set; }

        public Guid TutorId { get; set; }
        [ForeignKey("TutorId")]
        public Tutor? Tutor { get; set; }

        public DateTime FechaEmision { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string MetodoPagoSAT { get; set; } = ""; // PUE o PPD
        public string FormaPagoSAT { get; set; } = ""; // 01, 03, 04...

        public string Estatus { get; set; } = "Borrador"; // Borrador, Timbrado, Cancelado

        public string XmlCrudo { get; set; } = ""; // Aquí guardaremos el XML que generemos hoy

        // Estos se llenarán después cuando nos conectemos al PAC
        public string? Uuid { get; set; }
        public string? XmlTimbrado { get; set; }
    }
}