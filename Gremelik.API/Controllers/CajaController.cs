using Gremelik.API.Services;
using Gremelik.core.Entities;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Gremelik.core.DTOs;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Cajero")]
    public class CajaController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;
        private readonly CalculadoraDeudasService _calculadoraDeudas;

        public class ExcepcionAuditoriaDto
        {
            public Guid Id { get; set; }
            public DateTime FechaRegistro { get; set; }
            public string Usuario { get; set; } = "";
            public string AlumnoNombre { get; set; } = "";
            public string Matricula { get; set; } = "";
            public string FolioPago { get; set; } = "";
            public string Concepto { get; set; } = "";
            public string Motivo { get; set; } = "";
            public decimal BecaRestaurada { get; set; }
            public decimal RecargoPerdonado { get; set; }
        }

        public CajaController(GremelikDbContext context, CurrentTenantService tenantService, CalculadoraDeudasService calculadoraDeudas)
        {
            _context = context;
            _tenantService = tenantService;
            _calculadoraDeudas = calculadoraDeudas; // Inyectamos el cerebro
        }

        public class ConceptoAPagarCajaDto
        {
            public Guid CuentaPorCobrarId { get; set; }
            public decimal MontoAPagar { get; set; }
            public decimal DescuentoBecaFinalCalculado { get; set; }

            public bool EsRecargoVirtual { get; set; }
            public Guid? DeudaOriginalId { get; set; }
            public Guid? ConceptoPagoId { get; set; }
            public string ConceptoNombreVirtual { get; set; } = "";

            public bool ExcepcionBecaActivada { get; set; }
            public bool ExcepcionRecargoActivado { get; set; }
            public string? MotivoExcepcion { get; set; }
        }

        public class NuevoPagoDto
        {
            public Guid AlumnoId { get; set; }
            public int CicloId { get; set; }
            public int MetodoPago { get; set; }
            public decimal DineroRecibido { get; set; }
            public string? Comentarios { get; set; }
            public List<ConceptoAPagarCajaDto> ConceptosAPagar { get; set; } = new();
            public bool RequiereFactura { get; set; }
            public Guid? TutorId { get; set; }

            public string? Banco { get; set; }
            public string? TerminacionTarjeta { get; set; }
            public string? Autorizacion { get; set; }
        }

        [HttpGet("tutores/{alumnoId}")]
        public async Task<ActionResult<IEnumerable<Tutor>>> GetTutoresPorAlumno(Guid alumnoId)
        {
            var tutores = await (from r in _context.Set<RelacionAlumnoTutor>()
                                 join t in _context.Set<Tutor>() on r.TutorId equals t.Id
                                 where r.AlumnoId == alumnoId && r.Activo && t.Activo
                                 select t).ToListAsync();
            return Ok(tutores);
        }

        // Reemplaza TODO tu método GetDeudasCaja por este (¡mira qué limpio quedó!):
        [HttpGet("deudas/{alumnoId}/ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<DeudaCalculadaDto>>> GetDeudasCaja(Guid alumnoId, int cicloId)
        {
            // Le pedimos al cerebro que nos calcule las deudas a la fecha de HOY
            var resultados = await _calculadoraDeudas.CalcularDeudasAsync(alumnoId, cicloId, DateTime.Today);
            return Ok(resultados);
        }

        [HttpPost("cobrar")]
        public async Task<IActionResult> Cobrar([FromBody] NuevoPagoDto dto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            if (dto.ConceptosAPagar.Count == 0) return BadRequest("No hay conceptos seleccionados para pagar.");
            if (dto.DineroRecibido <= 0) return BadRequest("El monto a cobrar debe ser mayor a cero.");

            var escuelaId = _tenantService.TenantId.Value;
            var usuario = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Sistema";

            // Calculamos cuánto era la deuda real total de lo que seleccionaron en pantalla
            var deudaTotalSeleccionada = dto.ConceptosAPagar.Sum(x => x.MontoAPagar);

            // Validamos que no nos estén pagando de más de forma global
            if (dto.DineroRecibido > deudaTotalSeleccionada + 0.01m)
            {
                return BadRequest("El dinero recibido no puede ser mayor a la deuda total seleccionada.");
            }

            // CORRECCIÓN CLAVE: El recibo se emite por lo que realmente pagó el cliente (abono), no por lo que debía
            var totalTicketReal = dto.DineroRecibido;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevoPago = new Pago
                {
                    EscuelaId = escuelaId,
                    Usuario = usuario,
                    AlumnoId = dto.AlumnoId,
                    CicloEscolarId = dto.CicloId,
                    FechaPago = DateTime.Now,
                    TotalPagado = totalTicketReal, // <--- AHORA DICE LA VERDAD
                    DineroRecibido = dto.DineroRecibido,
                    Cambio = 0,
                    MetodoPago = (MetodoPago)dto.MetodoPago,
                    Banco = dto.Banco,
                    TerminacionTarjeta = dto.TerminacionTarjeta,
                    Autorizacion = dto.Autorizacion,
                    Comentarios = dto.Comentarios,
                    RequiereFactura = dto.RequiereFactura,
                    TutorId = dto.RequiereFactura ? dto.TutorId : null,
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Pagos.Add(nuevoPago);
                await _context.SaveChangesAsync();

                // LÓGICA DE ABONOS: Distribuir el dinero recibido entre las deudas seleccionadas
                decimal dineroSobranteParaRepartir = totalTicketReal;

                // NUEVO: JERARQUÍA DE COBRO. Ordenamos para que los recargos sean lo primero en cobrarse
                var conceptosOrdenadosParaCobro = dto.ConceptosAPagar
                    .OrderByDescending(c => c.EsRecargoVirtual || c.ConceptoNombreVirtual.StartsWith("Recargo"))
                    .ToList();

                foreach (var item in dto.ConceptosAPagar)
                {
                    if (dineroSobranteParaRepartir <= 0) break; // Si ya se nos acabó el dinero del abono, dejamos de abonar

                    // ¿Cuánto le toca a esta deuda? Si alcanza para pagarla toda, la pagamos. Si no, le damos lo que sobra (abono parcial)
                    decimal montoAbonarAEstaDeuda = Math.Min(item.MontoAPagar, dineroSobranteParaRepartir);

                    if (item.EsRecargoVirtual)
                    {
                        if (item.ExcepcionRecargoActivado)
                        {
                            var excepcion = new ExcepcionCaja
                            {
                                PagoId = nuevoPago.Id,
                                CuentaPorCobrarId = item.DeudaOriginalId!.Value,
                                Motivo = item.MotivoExcepcion ?? "Recargo perdonado manual",
                                RecargoPerdonadoMonto = item.MontoAPagar,
                                EscuelaId = escuelaId,
                                Usuario = usuario,
                                FechaRegistro = DateTime.Now,
                                Activo = true
                            };
                            _context.ExcepcionesCaja.Add(excepcion);

                            // Un recargo perdonado no consume dinero del abono real, así que NO restamos dineroSobranteParaRepartir
                        }
                        else
                        {
                            var recargoReal = new CuentaPorCobrar
                            {
                                EscuelaId = escuelaId,
                                Usuario = usuario,
                                AlumnoId = dto.AlumnoId,
                                CicloEscolarId = dto.CicloId,
                                ConceptoNombre = item.ConceptoNombreVirtual,
                                ConceptoPagoId = item.ConceptoPagoId,
                                FechaVencimiento = DateTime.Today,
                                MontoBase = item.MontoAPagar, // El recargo costaba 150
                                DescuentoBeca = 0,
                                NumeroDePago = 0,
                                TotalPagado = montoAbonarAEstaDeuda, // Le abonamos lo que alcanzó
                                Estado = montoAbonarAEstaDeuda >= item.MontoAPagar ? "PAGADO" : "PARCIAL", // Si dio $50, se queda parcial
                                EsFacturable = true,
                                FechaRegistro = DateTime.Now,
                                Activo = true
                            };
                            _context.CuentasPorCobrar.Add(recargoReal);
                            await _context.SaveChangesAsync();

                            var detalle = new DetallePago
                            {
                                PagoId = nuevoPago.Id,
                                CuentaPorCobrarId = recargoReal.Id,
                                MontoAbonado = montoAbonarAEstaDeuda, // <--- CORRECCIÓN DE DETALLE
                                ConceptoNombreSnapshot = recargoReal.ConceptoNombre,
                                Usuario = usuario,
                                FechaRegistro = DateTime.Now,
                                Activo = true
                            };
                            _context.DetallesPagos.Add(detalle);

                            dineroSobranteParaRepartir -= montoAbonarAEstaDeuda; // Descontamos el dinero que usamos
                        }
                    }
                    else // Es una deuda normal (Colegiatura, Libros)
                    {
                        var deuda = await _context.CuentasPorCobrar.FindAsync(item.CuentaPorCobrarId);
                        if (deuda == null) throw new Exception("Cuenta original no encontrada");

                        deuda.DescuentoBeca = item.DescuentoBecaFinalCalculado;

                        if (item.ExcepcionBecaActivada)
                        {
                            var excepcion = new ExcepcionCaja
                            {
                                PagoId = nuevoPago.Id,
                                CuentaPorCobrarId = deuda.Id,
                                Motivo = item.MotivoExcepcion ?? "Beca restaurada manual",
                                BecaRestauradaMonto = item.DescuentoBecaFinalCalculado,
                                EscuelaId = escuelaId,
                                Usuario = usuario,
                                FechaRegistro = DateTime.Now,
                                Activo = true
                            };
                            _context.ExcepcionesCaja.Add(excepcion);
                        }

                        var detalle = new DetallePago
                        {
                            PagoId = nuevoPago.Id,
                            CuentaPorCobrarId = deuda.Id,
                            MontoAbonado = montoAbonarAEstaDeuda, // <--- CORRECCIÓN
                            ConceptoNombreSnapshot = deuda.ConceptoNombre,
                            Usuario = usuario,
                            FechaRegistro = DateTime.Now,
                            Activo = true
                        };
                        _context.DetallesPagos.Add(detalle);

                        deuda.TotalPagado += montoAbonarAEstaDeuda; // Le sumamos solo el abono
                        deuda.Estado = deuda.TotalPagado >= (deuda.MontoBase - deuda.DescuentoBeca) ? "PAGADO" : "PARCIAL";
                        deuda.FUM = DateTime.Now;

                        dineroSobranteParaRepartir -= montoAbonarAEstaDeuda; // Descontamos el dinero que usamos
                    }
                }

                await _context.SaveChangesAsync();

                // Lógica de facturación original...
                // --- LÓGICA DE FACTURACIÓN INMEDIATA (CORREGIDA) ---
                if (dto.RequiereFactura && dto.TutorId.HasValue)
                {
                    // 1. Buscamos los datos completos del alumno (con Grado y Nivel para el RVOE)
                    var alumnoFac = await _context.Alumnos
                        .Include(a => a.Inscripciones)
                            .ThenInclude(i => i.Grado)
                                .ThenInclude(g => g.NivelEducativo)
                        .FirstOrDefaultAsync(a => a.Id == dto.AlumnoId);

                    var plantelId = alumnoFac?.Inscripciones.FirstOrDefault(i => i.Activo)?.PlantelId;
                    var plantel = await _context.Planteles.FindAsync(plantelId);
                    var tutor = await _context.Tutores.FindAsync(dto.TutorId.Value);

                    // 2. Filtramos solo los conceptos facturables que se están pagando ahorita
                    var idsCuentasSeleccionadas = dto.ConceptosAPagar.Select(c => c.CuentaPorCobrarId).ToList();
                    var deudasOriginales = await _context.CuentasPorCobrar
                        .Where(c => idsCuentasSeleccionadas.Contains(c.Id) && c.EsFacturable)
                        .ToListAsync();

                    // Obtenemos los detalles recién creados en esta transacción que coinciden con conceptos facturables
                    var detallesFacturables = _context.DetallesPagos.Local
                        .Where(d => deudasOriginales.Select(o => o.Id).Contains(d.CuentaPorCobrarId))
                        .ToList();

                    if (detallesFacturables.Any())
                    {
                        var cfdiBuilder = new CfdiBuilderService();
                        string xmlGenerado = cfdiBuilder.GenerarXmlCrudo(nuevoPago, plantel!, tutor!, detallesFacturables, deudasOriginales);

                        if (!string.IsNullOrEmpty(xmlGenerado))
                        {
                            // 3. SIMULAMOS EL TIMBRADO DE UNA VEZ (UUID y Estatus)
                            string fakeUuid = Guid.NewGuid().ToString().ToUpper();

                            var nuevaFactura = new Factura
                            {
                                PagoId = nuevoPago.Id,
                                TutorId = tutor!.Id,
                                FechaEmision = DateTime.Now,
                                SubTotal = detallesFacturables.Sum(d => d.MontoAbonado),
                                Total = detallesFacturables.Sum(d => d.MontoAbonado),
                                MetodoPagoSAT = "PUE",
                                FormaPagoSAT = cfdiBuilder.ObtenerFormaPagoSat(nuevoPago.MetodoPago),
                                Estatus = "Timbrada", // <--- CAMBIO: Ya no es borrador
                                XmlCrudo = xmlGenerado,
                                Uuid = fakeUuid,      // <--- CAMBIO: Ya lleva su sello
                                Usuario = usuario
                            };

                            _context.Facturas.Add(nuevaFactura);

                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await transaction.CommitAsync();

                return Ok(new { mensaje = "Cobro exitoso", folio = nuevoPago.Folio, pagoId = nuevoPago.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("auditoria/excepciones/{cicloId}")]
        public async Task<ActionResult<IEnumerable<ExcepcionAuditoriaDto>>> GetExcepcionesAuditoria(int cicloId)
        {
            var excepciones = await _context.ExcepcionesCaja
                .Include(e => e.Pago)
                    .ThenInclude(p => p.Alumno)
                .Include(e => e.CuentaPorCobrar)
                .Where(e => e.Pago!.CicloEscolarId == cicloId && e.Activo)
                .OrderByDescending(e => e.FechaRegistro)
                .Select(e => new ExcepcionAuditoriaDto
                {
                    Id = e.Id,
                    FechaRegistro = e.FechaRegistro,
                    Usuario = e.Usuario,
                    AlumnoNombre = e.Pago!.Alumno!.Nombre + " " + e.Pago!.Alumno!.PrimerApellido,
                    Matricula = e.Pago!.Alumno!.Matricula,
                    FolioPago = e.Pago!.Folio.ToString(),
                    Concepto = e.CuentaPorCobrar!.ConceptoNombre,
                    Motivo = e.Motivo,
                    BecaRestaurada = e.BecaRestauradaMonto,
                    RecargoPerdonado = e.RecargoPerdonadoMonto
                })
                .ToListAsync();

            return Ok(excepciones);
        }

        [HttpGet("historial")]
        public async Task<ActionResult<List<HistorialPagoDto>>> GetHistorialPagos(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] Guid? plantelId,
            [FromQuery] Guid? nivelId,
            [FromQuery] int? gradoId,
            [FromQuery] int? grupoId,
            [FromQuery] int? cicloId,
            [FromQuery] string? busqueda)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var inicio = fechaInicio.Date;
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

            // NUEVO: Agregamos Include para la Factura y así saber si ya está timbrado
            var query = _context.Pagos
                .Include(p => p.Alumno)
                    .ThenInclude(a => a.Inscripciones)
                .Include(p => p.Detalles)
                .Where(p => p.EscuelaId == tenantId && p.FechaPago >= inicio && p.FechaPago <= fin);

            if (cicloId.HasValue && cicloId.Value > 0)
            {
                query = query.Where(p => p.CicloEscolarId == cicloId.Value);
            }

            if (plantelId.HasValue || gradoId.HasValue || grupoId.HasValue)
            {
                query = query.Where(p => p.Alumno!.Inscripciones.Any(i => i.Activo &&
                    (!cicloId.HasValue || i.CicloEscolarId == cicloId) &&
                    (!plantelId.HasValue || i.PlantelId == plantelId) &&
                    (!gradoId.HasValue || i.GradoId == gradoId) &&
                    (!grupoId.HasValue || i.GrupoId == grupoId)
                ));
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var b = busqueda.ToLower().Trim();
                if (int.TryParse(b, out int folioBusqueda))
                {
                    query = query.Where(p => p.Folio == folioBusqueda ||
                                             p.Alumno!.Matricula.ToLower().Contains(b) ||
                                             (p.Alumno.Nombre + " " + p.Alumno.PrimerApellido).ToLower().Contains(b));
                }
                else
                {
                    query = query.Where(p => p.Alumno!.Matricula.ToLower().Contains(b) ||
                                             (p.Alumno.Nombre + " " + p.Alumno.PrimerApellido).ToLower().Contains(b));
                }
            }

            var resultados = await query
                .OrderByDescending(p => p.FechaPago)
                .Select(p => new HistorialPagoDto
                {
                    Id = p.Id,
                    Folio = p.Folio,
                    FechaPago = p.FechaPago,
                    AlumnoId = p.AlumnoId,
                    Matricula = p.Alumno!.Matricula,
                    AlumnoNombre = $"{p.Alumno.Nombre} {p.Alumno.PrimerApellido} {p.Alumno.SegundoApellido}".Trim(),
                    TotalPagado = p.TotalPagado,
                    MetodoPago = p.MetodoPago.ToString(),
                    Usuario = p.Usuario,
                    RequiereFactura = p.RequiereFactura,
                    Conceptos = string.Join(", ", p.Detalles.Where(d => d.Activo).Select(d => d.ConceptoNombreSnapshot)),
                    Cancelado = !p.Activo,

                    // --- BANDERAS FISCALES EVALUADAS AL VUELO ---
                    PuedeFacturarse = _context.CuentasPorCobrar.Where(c => p.Detalles.Select(d => d.CuentaPorCobrarId).Contains(c.Id)).Any(c => c.EsFacturable),

                    // ¿Existe la factura pero su UUID está vacío? (Borrador de Caja)
                    TieneFacturaBorrador = _context.Facturas.Any(f => f.PagoId == p.Id && (f.Uuid == null || f.Uuid == "")),

                    // ¿Existe y ya tiene UUID? (Timbrada)
                    Timbrado = _context.Facturas.Any(f => f.PagoId == p.Id && f.Uuid != null && f.Uuid != ""),

                    Uuid = _context.Facturas.Where(f => f.PagoId == p.Id).Select(f => f.Uuid).FirstOrDefault()
                })
                .Take(200)
                .ToListAsync();

            return Ok(resultados);
        }

        public class CancelarPagoDto { public Guid PagoId { get; set; } public string Motivo { get; set; } = ""; }

        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarPago([FromBody] CancelarPagoDto dto)
        {
            var pago = await _context.Pagos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == dto.PagoId);

            if (pago == null) return NotFound("Pago no encontrado.");
            if (!pago.Activo) return BadRequest("El pago ya estaba cancelado.");

            // NUEVO (Paso 3): Buscamos si este pago tiene una factura asociada
            var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.PagoId == pago.Id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                pago.Activo = false;
                pago.Comentarios = $"CANCELADO: {dto.Motivo} | " + (pago.Comentarios ?? "");
                pago.FUM = DateTime.Now;

                // LÓGICA FISCAL (Paso 3): 
                // Si existe factura, cambiamos su estatus. 
                // Nota: Aquí es donde más adelante dispararemos la cancelación oficial al PAC.
                if (factura != null)
                {
                    factura.Estatus = "Cancelada";
                    factura.Usuario = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Sistema";
                    // Si ya estaba timbrada (tenía UUID), podrías guardar el motivo de cancelación del SAT aquí
                }

                // Revertir el dinero de las cuentas por cobrar (Tu lógica original intacta)
                foreach (var det in pago.Detalles)
                {
                    det.Activo = false;

                    var deuda = await _context.CuentasPorCobrar.FindAsync(det.CuentaPorCobrarId);
                    if (deuda != null)
                    {
                        deuda.TotalPagado -= det.MontoAbonado;
                        if (deuda.TotalPagado <= 0) deuda.TotalPagado = 0;

                        deuda.Estado = deuda.TotalPagado == 0 ? "PENDIENTE" : "PARCIAL";
                        deuda.FUM = DateTime.Now;

                        if (deuda.NumeroDePago == 0 && deuda.TotalPagado == 0)
                        {
                            deuda.Activo = false;
                        }
                    }
                }

                // Inactivar excepciones asociadas
                var excepciones = await _context.ExcepcionesCaja.Where(e => e.PagoId == pago.Id).ToListAsync();
                foreach (var exc in excepciones) { exc.Activo = false; }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error interno al cancelar: {ex.Message}");
            }
        }

        [HttpGet("xml/{pagoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> DescargarXmlPago(Guid pagoId)
        {
            var factura = await _context.Facturas.FirstOrDefaultAsync(f => f.PagoId == pagoId);

            if (factura == null || string.IsNullOrEmpty(factura.XmlCrudo))
                return Content("<error>No se encontró el XML generado para este pago.</error>", "application/xml");

            // Al retornar Content con "application/xml", el navegador lo abre y lo formatea bonito en la pestaña en lugar de descargarlo.
            return Content(factura.XmlCrudo, "application/xml");
        }
    }
}