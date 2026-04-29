public class HistorialPagoDto
{
    public Guid Id { get; set; }
    public int Folio { get; set; }
    public DateTime FechaPago { get; set; }
    public string Matricula { get; set; } = "";
    public string AlumnoNombre { get; set; } = "";
    public decimal TotalPagado { get; set; }
    public string MetodoPago { get; set; } = "";
    public string Usuario { get; set; } = "";
    public bool RequiereFactura { get; set; }

    // --- NUEVOS CAMPOS ---
    public string Conceptos { get; set; } = "";
    public bool Cancelado { get; set; }
    public Guid AlumnoId { get; set; }  // Para poder buscar a sus tutores
    public bool Timbrado { get; set; }  // Para saber si ya tiene factura en el SAT
    public string Uuid { get; set; }    // Para mostrar el folio fiscal
    public bool PuedeFacturarse { get; set; }
    public bool TieneFacturaBorrador { get; set; } // <--- LA NUEVA
}
