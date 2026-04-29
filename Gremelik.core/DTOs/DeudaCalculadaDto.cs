namespace Gremelik.core.DTOs
{
    public class DeudaCalculadaDto
    {
        public Guid CuentaPorCobrarId { get; set; }
        public string ConceptoNombre { get; set; } = "";
        public int NumeroDePago { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoBase { get; set; }
        public decimal DescuentoBecaOriginal { get; set; }
        public decimal DescuentoBecaAplicado { get; set; }
        public decimal TotalPagado { get; set; }
        public string Estado { get; set; } = "";
        public bool EsFacturable { get; set; }
        public bool BecaPerdidaPorAtraso { get; set; }
        public bool EsRecargoVirtual { get; set; }
        public Guid? DeudaOriginalId { get; set; }
        public Guid? ConceptoPagoId { get; set; }
    }
}