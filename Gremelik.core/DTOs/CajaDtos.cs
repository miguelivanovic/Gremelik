using System;
using System.Collections.Generic;

namespace Gremelik.core.DTOs
{
    // Esta clase ahora es visible para la API y para la WEB
    public class NuevoPagoDto
    {
        public Guid AlumnoId { get; set; }
        public int CicloId { get; set; }
        public int MetodoPago { get; set; }
        public decimal DineroRecibido { get; set; }
        public string? Comentarios { get; set; }
        public List<DetallePagoDto> ConceptosAPagar { get; set; } = new();

        // --- NUEVOS CAMPOS ---
        public bool RequiereFactura { get; set; }
        public Guid? TutorId { get; set; }
    }

    public class DetallePagoDto
    {
        public Guid CuentaPorCobrarId { get; set; }
        public decimal MontoAPagar { get; set; }
    }

    // DTO PRINCIPAL DEL CORTE DE CAJA
    public class CorteCajaDto
    {
        public DateTime FechaConsulta { get; set; }
        public decimal TotalCobrado { get; set; }
        public List<ResumenMetodoPagoDto> ResumenPorMetodo { get; set; } = new();
        public List<PagoCorteDto> DetallePagos { get; set; } = new();
    }

    // DTO PARA AGRUPAR POR EFECTIVO, TARJETA, ETC.
    public class ResumenMetodoPagoDto
    {
        public string MetodoPago { get; set; } = "";
        public decimal Total { get; set; }
        public int CantidadOperaciones { get; set; }
    }

    // DTO PARA LA TABLA DEL DESGLOSE DE RECIBOS
    public class PagoCorteDto
    {
        public Guid PagoId { get; set; }
        public string Folio { get; set; } = "";
        public DateTime FechaPago { get; set; }
        public string AlumnoNombre { get; set; } = "";
        public string MetodoPago { get; set; } = "";
        public decimal Total { get; set; }
        public string Usuario { get; set; } = "";
        public bool RequiereFactura { get; set; }
    }

    public class AlumnoMorosoDto
    {
        public Guid AlumnoId { get; set; }
        public string Matricula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public decimal DeudaTotal { get; set; }
        public int CantidadConceptosVencidos { get; set; }
        public DateTime FechaDeudaMasAntigua { get; set; }
    }

    public class IngresoPorConceptoDto
    {
        public string Concepto { get; set; } = "";
        public decimal TotalCobrado { get; set; }
        public int CantidadOperaciones { get; set; }
    }

    // DTO PRINCIPAL DEL DASHBOARD FINANCIERO
    public class ResumenFinancieroGlobalDto
    {
        public int CicloEscolarId { get; set; }
        public string CicloNombre { get; set; } = "";

        // Lo que ya entró al banco
        public decimal TotalCobrado { get; set; }

        // Lo que ya venció y no han pagado (Urgente)
        public decimal TotalVencido { get; set; }

        // Lo que está programado para cobrarse en el futuro (Proyección)
        public decimal TotalPorCobrarFuturo { get; set; }

        // La suma de todo (Lo que vale el ciclo escolar)
        public decimal ValorTotalCiclo => TotalCobrado + TotalVencido + TotalPorCobrarFuturo;

        // Desglose para gráficas
        public List<DesgloseFinancieroMensualDto> DesgloseMensual { get; set; } = new();
    }

    public class DesgloseFinancieroMensualDto
    {
        public string Mes { get; set; } = "";
        public int NumeroMes { get; set; }

        public decimal Cobrado { get; set; }
        public decimal Vencido { get; set; }
        public decimal Pendiente { get; set; }

        // --- NUEVO: MATEMÁTICA PARA GRÁFICAS Y BARRAS DE AVANCE ---
        public decimal TotalEsperadoDelMes => Cobrado + Vencido + Pendiente;

        public double PorcentajeAvance => TotalEsperadoDelMes > 0
            ? (double)(Cobrado / TotalEsperadoDelMes) * 100
            : 0;

        public double PorcentajeVencido => TotalEsperadoDelMes > 0
            ? (double)(Vencido / TotalEsperadoDelMes) * 100
            : 0;
    }

    public class ConceptoFinancieroMensualDto
    {
        public string Concepto { get; set; } = "";
        public decimal Cobrado { get; set; }
        public decimal Vencido { get; set; }
        public decimal Pendiente { get; set; }

        public decimal TotalEsperado => Cobrado + Vencido + Pendiente;
        public double PorcentajeAvance => TotalEsperado > 0 ? (double)(Cobrado / TotalEsperado) * 100 : 0;
    }

    public class AplicarSaldoAFavorDto
    {
        public Guid AlumnoId { get; set; }
        public List<Guid> CuentasPorCobrarIds { get; set; } = new();
    }
}