using System.Xml.Linq;
using Gremelik.core.Entities;

namespace Gremelik.core.Services
{
    public class CfdiBuilderService
    {
        public string GenerarXmlCrudo(Pago pago, Plantel plantel, Tutor tutor, List<DetallePago> detalles, List<CuentaPorCobrar> deudasOriginales)
        {
            XNamespace cfdi = "http://www.sat.gob.mx/cfd/4";
            XNamespace iedu = "http://www.sat.gob.mx/iedu";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var conceptosFacturables = deudasOriginales.Where(d => d.EsFacturable).ToList();
            if (!conceptosFacturables.Any()) return "";

            string formaPagoSat = ObtenerFormaPagoSat(pago.MetodoPago);
            decimal subTotal = conceptosFacturables.Sum(c => c.MontoBase - c.DescuentoBeca);
            decimal total = subTotal;

            var comprobante = new XElement(cfdi + "Comprobante",
                new XAttribute(XNamespace.Xmlns + "cfdi", cfdi.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "iedu", iedu.NamespaceName),
                new XAttribute(xsi + "schemaLocation", "http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd http://www.sat.gob.mx/iedu http://www.sat.gob.mx/sitio_internet/cfd/iedu/iedu.xsd"),

                new XAttribute("Version", "4.0"),
                new XAttribute("Serie", "F"),
                new XAttribute("Folio", pago.Folio.ToString()),
                new XAttribute("Fecha", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XAttribute("FormaPago", formaPagoSat),
                new XAttribute("NoCertificado", "AQUI_IRA_EL_NUMERO_DE_CERTIFICADO"),
                new XAttribute("Certificado", "AQUI_IRA_EL_CERTIFICADO_BASE64"),
                new XAttribute("SubTotal", subTotal.ToString("F2")),
                new XAttribute("Moneda", "MXN"),
                new XAttribute("TipoCambio", "1"),
                new XAttribute("Total", total.ToString("F2")),
                new XAttribute("TipoDeComprobante", "I"),
                new XAttribute("Exportacion", "01"),
                new XAttribute("MetodoPago", "PUE"),
                new XAttribute("LugarExpedicion", plantel.CodigoPostalFiscal ?? "35000"),

                new XElement(cfdi + "Emisor",
                    new XAttribute("Rfc", plantel.RFC ?? "XAXX010101000"),
                    new XAttribute("Nombre", plantel.RazonSocial ?? "ESCUELA"),
                    new XAttribute("RegimenFiscal", plantel.RegimenFiscal ?? "601")
                ),

                new XElement(cfdi + "Receptor",
                    new XAttribute("Rfc", tutor.RFC ?? "XAXX010101000"),
                    new XAttribute("Nombre", $"{tutor.Nombre} {tutor.PrimerApellido} {tutor.SegundoApellido}".Trim().ToUpper()),
                    new XAttribute("DomicilioFiscalReceptor", tutor.CodigoPostal ?? "00000"),
                    new XAttribute("RegimenFiscalReceptor", tutor.RegimenFiscal ?? "616"),
                    new XAttribute("UsoCFDI", tutor.UsoCFDI ?? "D10")
                )
            );

            var nodoConceptos = new XElement(cfdi + "Conceptos");
            decimal baseTotalExenta = 0;

            foreach (var concepto in conceptosFacturables)
            {
                var detallePago = detalles.FirstOrDefault(d => d.CuentaPorCobrarId == concepto.Id);
                decimal importeAbonado = detallePago?.MontoAbonado ?? 0;

                if (importeAbonado > 0)
                {
                    baseTotalExenta += importeAbonado;

                    var nodoConcepto = new XElement(cfdi + "Concepto",
                        new XAttribute("ClaveProdServ", "86121500"),
                        new XAttribute("NoIdentificacion", concepto.Id.ToString().Substring(0, 8).ToUpper()),
                        new XAttribute("Cantidad", "1"),
                        new XAttribute("ClaveUnidad", "E48"),
                        new XAttribute("Unidad", "SERVICIO"),
                        new XAttribute("Descripcion", concepto.ConceptoNombre.ToUpper()),
                        new XAttribute("ValorUnitario", importeAbonado.ToString("F2")),
                        new XAttribute("Importe", importeAbonado.ToString("F2")),
                        new XAttribute("ObjetoImp", "02")
                    );

                    var nodoImpuestos = new XElement(cfdi + "Impuestos");
                    var nodoTraslados = new XElement(cfdi + "Traslados");

                    nodoTraslados.Add(new XElement(cfdi + "Traslado",
                        new XAttribute("Base", importeAbonado.ToString("F2")),
                        new XAttribute("Impuesto", "002"),
                        new XAttribute("TipoFactor", "Exento")
                    ));

                    nodoImpuestos.Add(nodoTraslados);
                    nodoConcepto.Add(nodoImpuestos);

                    // --- COMPLEMENTO EDUCATIVO (IEDU) DINÁMICO ---
                    if (tutor.UsoCFDI == "D10" && concepto.ConceptoNombre.ToLower().Contains("colegiatura"))
                    {
                        string nombreAlumnoSat = $"{pago.Alumno?.PrimerApellido} {pago.Alumno?.SegundoApellido} {pago.Alumno?.Nombre}".Trim().ToUpper();

                        var inscripcion = pago.Alumno?.Inscripciones?.FirstOrDefault(i => i.Activo);
                        var nivel = inscripcion?.Grado?.NivelEducativo;

                        // Validamos que existan en BD, de lo contrario mandamos defaults seguros
                        string nivelSat = string.IsNullOrWhiteSpace(nivel?.NivelSAT) ? "Secundaria" : nivel.NivelSAT;
                        string rvoeSat = string.IsNullOrWhiteSpace(nivel?.RVOE) ? "PENDIENTE" : nivel.RVOE;

                        var nodoComplementoConcepto = new XElement(cfdi + "ComplementoConcepto");
                        var nodoInstEducativas = new XElement(iedu + "instEducativas",
                            new XAttribute("version", "1.0"),
                            new XAttribute("nombreAlumno", nombreAlumnoSat),
                            new XAttribute("CURP", pago.Alumno?.CURP ?? "XAXX010101XXXXXX00"),
                            new XAttribute("nivelEducativo", nivelSat),
                            new XAttribute("autRVOE", rvoeSat),
                            new XAttribute("rfcPago", tutor.RFC ?? "XAXX010101000")
                        );

                        nodoComplementoConcepto.Add(nodoInstEducativas);
                        nodoConcepto.Add(nodoComplementoConcepto);
                    }

                    nodoConceptos.Add(nodoConcepto);
                }
            }

            comprobante.Add(nodoConceptos);

            var nodoImpuestosGlobal = new XElement(cfdi + "Impuestos",
                new XElement(cfdi + "Traslados",
                    new XElement(cfdi + "Traslado",
                        new XAttribute("Base", baseTotalExenta.ToString("F2")),
                        new XAttribute("Impuesto", "002"),
                        new XAttribute("TipoFactor", "Exento")
                    )
                )
            );

            comprobante.Add(nodoImpuestosGlobal);

            return comprobante.ToString();
        }

        public string ObtenerFormaPagoSat(MetodoPago metodoPago)
        {
            return metodoPago switch
            {
                MetodoPago.Efectivo => "01",
                MetodoPago.TarjetaDebito => "28",
                MetodoPago.TarjetaCredito => "04",
                MetodoPago.Transferencia => "03",
                MetodoPago.Cheque => "02",
                _ => "99"
            };
        }
    }
}