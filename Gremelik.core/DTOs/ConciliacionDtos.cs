namespace Gremelik.core.DTOs
{
    // 1. El idioma universal: No importa si es Banorte o BBVA, todos deben entregar esto.
    public class TransaccionBancariaDto
    {
        public string Referencia { get; set; } = ""; // Aquí vendrá la Matrícula
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string ClaveRastreo { get; set; } = ""; // Folio de autorización del banco
        public string MetodoPago { get; set; } = "Transferencia";
    }

    // 2. El reporte final que verá el cajero en la pantalla al subir el archivo
    public class ResultadoConciliacionDto
    {
        public int ProcesadosExitosamente { get; set; }
        public decimal MontoTotalAplicado { get; set; }
        public List<string> Advertencias { get; set; } = new(); // Ej: "La referencia 999 no existe"
    }

    public class ResolverConciliacionDto
    {
        public Guid TransaccionId { get; set; }
        public Guid AlumnoId { get; set; }
        public List<DetalleConciliacionDto> Conceptos { get; set; } = new();
    }

    public class DetalleConciliacionDto
    {
        public Guid CuentaPorCobrarId { get; set; }
        public decimal MontoAAplicar { get; set; }

        // --- NUEVOS CAMPOS PARA SOPORTAR CASTIGOS ---
        public bool EsRecargoVirtual { get; set; }
        public Guid? DeudaOriginalId { get; set; }
        public Guid? ConceptoPagoId { get; set; }
        public string ConceptoNombreVirtual { get; set; } = "";
        public decimal DescuentoBecaFinalCalculado { get; set; }
    }
}
