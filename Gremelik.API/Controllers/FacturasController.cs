using Gremelik.API.Services;
using Gremelik.core.Entities;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Cajero")]
    public class FacturasController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public FacturasController(GremelikDbContext context)
        {
            _context = context;
        }

        public class TimbrarExistenteDto
        {
            public Guid PagoId { get; set; }
            public Guid TutorId { get; set; }
        }

        // 1. MÉTODO PARA EL BOTÓN GRIS (Facturar desde cero)
        [HttpPost("timbrar-existente")]
        public async Task<IActionResult> TimbrarExistente([FromBody] TimbrarExistenteDto dto)
        {
            var pago = await _context.Pagos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == dto.PagoId);

            if (pago == null) return NotFound("Pago no encontrado.");

            var tutor = await _context.Tutores.FindAsync(dto.TutorId);
            if (tutor == null) return BadRequest("Tutor no encontrado.");

            if (await _context.Facturas.AnyAsync(f => f.PagoId == dto.PagoId))
                return BadRequest("Este pago ya tiene un CFDI en proceso o timbrado.");

            // Filtrar solo los conceptos que la escuela marcó como facturables
            var detallesFacturables = await (from d in _context.DetallesPagos
                                             join c in _context.CuentasPorCobrar on d.CuentaPorCobrarId equals c.Id
                                             where d.PagoId == dto.PagoId && c.EsFacturable && d.Activo
                                             select d).ToListAsync();

            if (!detallesFacturables.Any())
                return BadRequest("Este pago no contiene conceptos facturables.");

            decimal montoFactura = detallesFacturables.Sum(x => x.MontoAbonado);

            // Extraer datos del colegio y del alumno
            var alumnoFac = await _context.Alumnos
                .Include(a => a.Inscripciones)
                    .ThenInclude(i => i.Grado)
                        .ThenInclude(g => g.NivelEducativo)
                .FirstOrDefaultAsync(a => a.Id == pago!.AlumnoId);

            var plantelId = alumnoFac?.Inscripciones.FirstOrDefault(i => i.Activo)?.PlantelId;
            var plantel = await _context.Planteles.FindAsync(plantelId);

            var idsCuentas = detallesFacturables.Select(c => c.CuentaPorCobrarId).ToList();
            var deudasOriginales = await _context.CuentasPorCobrar
                .Where(c => idsCuentas.Contains(c.Id))
                .ToListAsync();

            // --- LA MAGIA: USAMOS TU CFDI BUILDER PARA GENERAR EL XML CON COMPLEMENTO IEDU ---
            var cfdiBuilder = new CfdiBuilderService();
            string xmlGenerado = cfdiBuilder.GenerarXmlCrudo(pago, plantel!, tutor, detallesFacturables, deudasOriginales);

            if (string.IsNullOrEmpty(xmlGenerado))
                return BadRequest("Error interno: El CfdiBuilder no devolvió un XML válido.");

            // TODO: Aquí en el futuro conectarás con tu proveedor PAC
            string fakeUuid = Guid.NewGuid().ToString().ToUpper(); // Simulación temporal

            var nuevaFactura = new Factura
            {
                PagoId = pago.Id,
                TutorId = tutor.Id,
                FechaEmision = DateTime.Now,
                SubTotal = montoFactura,
                Total = montoFactura,
                MetodoPagoSAT = "PUE",
                FormaPagoSAT = cfdiBuilder.ObtenerFormaPagoSat(pago.MetodoPago),
                Estatus = "Timbrada",
                XmlCrudo = xmlGenerado,
                Uuid = fakeUuid,
                Usuario = User.Identity?.Name ?? "Sistema"
            };

            _context.Facturas.Add(nuevaFactura);

            pago.RequiereFactura = true;
            pago.TutorId = tutor.Id;

            await _context.SaveChangesAsync();
            return Ok(new { uuid = fakeUuid });
        }

        // 2. MÉTODO PARA EL BOTÓN AMARILLO (Timbrar factura borrador)
        [HttpPost("timbrar-borrador/{pagoId}")]
        public async Task<IActionResult> TimbrarBorrador(Guid pagoId)
        {
            var facturaBorrador = await _context.Facturas
                .Include(f => f.Tutor)
                .FirstOrDefaultAsync(f => f.PagoId == pagoId);

            if (facturaBorrador == null) return NotFound("No se encontró el borrador de la factura.");
            if (!string.IsNullOrEmpty(facturaBorrador.Uuid)) return BadRequest("Esta factura ya fue timbrada previamente.");

            var pago = await _context.Pagos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == pagoId);

            // Re-generamos el XML por seguridad, usando la misma lógica de arriba
            var detallesFacturables = await (from d in _context.DetallesPagos
                                             join c in _context.CuentasPorCobrar on d.CuentaPorCobrarId equals c.Id
                                             where d.PagoId == pagoId && c.EsFacturable && d.Activo
                                             select d).ToListAsync();

            var alumnoFac = await _context.Alumnos
                .Include(a => a.Inscripciones)
                    .ThenInclude(i => i.Grado)
                        .ThenInclude(g => g.NivelEducativo)
                .FirstOrDefaultAsync(a => a.Id == pago!.AlumnoId);

            var plantelId = alumnoFac?.Inscripciones.FirstOrDefault(i => i.Activo)?.PlantelId;
            var plantel = await _context.Planteles.FindAsync(plantelId);

            var idsCuentas = detallesFacturables.Select(c => c.CuentaPorCobrarId).ToList();
            var deudasOriginales = await _context.CuentasPorCobrar
                .Where(c => idsCuentas.Contains(c.Id))
                .ToListAsync();

            var cfdiBuilder = new CfdiBuilderService();
            string xmlGenerado = cfdiBuilder.GenerarXmlCrudo(pago!, plantel!, facturaBorrador.Tutor!, detallesFacturables, deudasOriginales);

            // TODO: Aquí conectarás con el PAC
            string fakeUuid = Guid.NewGuid().ToString().ToUpper();

            // Actualizamos el borrador con los datos definitivos
            facturaBorrador.XmlCrudo = xmlGenerado;
            facturaBorrador.Uuid = fakeUuid;
            facturaBorrador.Estatus = "Timbrada";
            facturaBorrador.FechaEmision = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { uuid = fakeUuid });
        }

        // RUTAS PARA DESCARGAS
        [HttpGet("xml/{pagoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetXml(Guid pagoId)
        {
            var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.PagoId == pagoId);
            if (factura == null || string.IsNullOrEmpty(factura.XmlCrudo)) return NotFound("XML no encontrado.");
            return Content(factura.XmlCrudo, "application/xml");
        }

        [HttpGet("pdf/{pagoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPdf(Guid pagoId)
        {
            return Redirect($"/api/Reportes/recibo/{pagoId}");
        }
    }
}