using Gremelik.core.Entities;
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class CuentasController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public CuentasController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // DTO PARA RECIBIR LA ORDEN DE GENERACIÓN
        // 1. AGREGA ESTE CAMPO AL DTO
        public class GeneracionCargosDto
        {
            public Guid AlumnoId { get; set; }
            public int CicloId { get; set; }
            public Guid PlanPagoId { get; set; }
            public Guid? BecaId { get; set; }
            public List<Guid> ServiciosAdicionalesIds { get; set; } = new();
            public List<Guid> ConceptosUnicosIds { get; set; } = new(); // <--- IMPORTANTE
            public DateTime? FechaInicioCobro { get; set; }
        }

        [HttpPost("generar")]
        public async Task<IActionResult> GenerarCargos([FromBody] GeneracionCargosDto dto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            // 1. VARIABLES GLOBALES DEL MÉTODO (Aquí definimos todo para que no marque error abajo)
            var escuelaId = _tenantService.TenantId.Value;
            var usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
            var ciclo = await _context.CiclosEscolares.FindAsync(dto.CicloId);
            var plan = await _context.PlanesPago.Include(p => p.ConceptoRelacionado).FirstOrDefaultAsync(p => p.Id == dto.PlanPagoId);

            if (alumno == null || ciclo == null || plan == null) return BadRequest("Datos inválidos.");

            Beca? beca = null;
            if (dto.BecaId.HasValue) beca = await _context.Becas.FindAsync(dto.BecaId);

            // Fecha de corte
            DateTime fechaMinimaCobro = dto.FechaInicioCobro ?? ciclo.FechaInicio;
            fechaMinimaCobro = new DateTime(fechaMinimaCobro.Year, fechaMinimaCobro.Month, 1);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ---------------------------------------------------------
                // A) GENERAR COLEGIATURAS (PLAN DE PAGO)
                // ---------------------------------------------------------
                if (plan.ConceptoRelacionado != null)
                {
                    decimal montoAnual = plan.ConceptoRelacionado.Monto;
                    decimal montoMensual = montoAnual / plan.NumeroPagos;

                    decimal descuentoMensual = 0;
                    if (beca != null && plan.ConceptoRelacionado.AplicaBeca && beca.AplicaEnColegiatura)
                    {
                        if (beca.Porcentaje > 0) descuentoMensual = montoMensual * (beca.Porcentaje / 100);
                        else descuentoMensual = beca.MontoFijo;
                    }

                    var mesesDobles = plan.MesesDobleCobro?
                        .Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int n) ? n : 0)
                        .Where(n => n > 0).ToList() ?? new List<int>();

                    DateTime fechaBase = ciclo.FechaInicio;

                    for (int i = 1; i <= plan.NumeroPagos; i++)
                    {
                        DateTime fechaVencimiento = new DateTime(fechaBase.Year, fechaBase.Month, Math.Min(plan.DiaLimitePago, DateTime.DaysInMonth(fechaBase.Year, fechaBase.Month)));
                        var fechaMesVencimiento = new DateTime(fechaVencimiento.Year, fechaVencimiento.Month, 1);

                        // Filtro de fecha (Noviembre en adelante)
                        if (fechaMesVencimiento >= fechaMinimaCobro)
                        {
                            var cargo = new CuentaPorCobrar
                            {
                                EscuelaId = escuelaId,
                                Usuario = usuario,
                                AlumnoId = alumno.Id,
                                CicloEscolarId = ciclo.Id,
                                ConceptoNombre = $"{plan.ConceptoRelacionado.Nombre} - {fechaVencimiento:MMMM yyyy}",
                                ConceptoPagoId = plan.ConceptoRelacionado.Id,
                                FechaVencimiento = fechaVencimiento,
                                MontoBase = montoMensual,
                                DescuentoBeca = descuentoMensual,
                                NumeroDePago = i,
                                BecaId = beca?.Id,
                                Estado = "PENDIENTE",
                                FechaRegistro = DateTime.Now,
                                EsFacturable = plan.ConceptoRelacionado.EsFacturable,
                                Activo = true
                            };
                            _context.CuentasPorCobrar.Add(cargo);

                            if (mesesDobles.Contains(fechaVencimiento.Month))
                            {
                                var cargoDoble = new CuentaPorCobrar
                                {
                                    EscuelaId = escuelaId,
                                    Usuario = usuario,
                                    AlumnoId = alumno.Id,
                                    CicloEscolarId = ciclo.Id,
                                    ConceptoNombre = $"{plan.ConceptoRelacionado.Nombre} (Extra) - {fechaVencimiento:MMMM yyyy}",
                                    ConceptoPagoId = plan.ConceptoRelacionado.Id,
                                    FechaVencimiento = fechaVencimiento,
                                    MontoBase = montoMensual,
                                    DescuentoBeca = descuentoMensual,
                                    NumeroDePago = i,
                                    BecaId = beca?.Id,
                                    Estado = "PENDIENTE",
                                    FechaRegistro = DateTime.Now,
                                    Activo = true
                                };
                                _context.CuentasPorCobrar.Add(cargoDoble);
                            }
                        }
                        fechaBase = fechaBase.AddMonths(1);
                    }
                }

                // ---------------------------------------------------------
                // B) GENERAR SERVICIOS ADICIONALES (MENSUALES)
                // ---------------------------------------------------------
                if (dto.ServiciosAdicionalesIds.Any())
                {
                    var servicios = await _context.ConceptosPago.Where(c => dto.ServiciosAdicionalesIds.Contains(c.Id)).ToListAsync();

                    foreach (var servicio in servicios)
                    {
                        DateTime fechaBaseServicio = ciclo.FechaInicio;
                        int mesesGenerar = plan.NumeroPagos > 0 ? plan.NumeroPagos : 10;

                        for (int i = 1; i <= mesesGenerar; i++)
                        {
                            DateTime fechaVencimiento = new DateTime(fechaBaseServicio.Year, fechaBaseServicio.Month, Math.Min(plan.DiaLimitePago, DateTime.DaysInMonth(fechaBaseServicio.Year, fechaBaseServicio.Month)));
                            var fechaMesVencimiento = new DateTime(fechaVencimiento.Year, fechaVencimiento.Month, 1);

                            if (fechaMesVencimiento >= fechaMinimaCobro)
                            {
                                var cargoServicio = new CuentaPorCobrar
                                {
                                    EscuelaId = escuelaId,
                                    Usuario = usuario,
                                    AlumnoId = alumno.Id,
                                    CicloEscolarId = ciclo.Id,
                                    ConceptoNombre = $"{servicio.Nombre} - {fechaVencimiento:MMMM}",
                                    ConceptoPagoId = servicio.Id,
                                    FechaVencimiento = fechaVencimiento,
                                    MontoBase = servicio.Monto,
                                    DescuentoBeca = 0,
                                    NumeroDePago = i,
                                    Estado = "PENDIENTE",
                                    FechaRegistro = DateTime.Now,
                                    EsFacturable = servicio.EsFacturable,
                                    Activo = true
                                };
                                _context.CuentasPorCobrar.Add(cargoServicio);
                            }
                            fechaBaseServicio = fechaBaseServicio.AddMonths(1);
                        }
                    }
                }

                // ---------------------------------------------------------
                // C) GENERAR PAGOS ÚNICOS (LIBROS, UNIFORMES)
                // ---------------------------------------------------------
                // AQUÍ ES DONDE TE DABA ERROR ANTES: Ahora 'alumno', 'ciclo' y 'usuario' ya existen.
                if (dto.ConceptosUnicosIds.Any())
                {
                    var unicos = await _context.ConceptosPago.Where(c => dto.ConceptosUnicosIds.Contains(c.Id)).ToListAsync();

                    foreach (var item in unicos)
                    {
                        DateTime fechaVencimiento = dto.FechaInicioCobro ?? DateTime.Now;

                        var cargoUnico = new CuentaPorCobrar
                        {
                            EscuelaId = escuelaId,
                            Usuario = usuario,
                            AlumnoId = alumno.Id,
                            CicloEscolarId = ciclo.Id,
                            ConceptoNombre = item.Nombre, // Ej: "Libros 1ro"
                            ConceptoPagoId = item.Id,
                            FechaVencimiento = fechaVencimiento,
                            MontoBase = item.Monto,
                            DescuentoBeca = 0,
                            NumeroDePago = 1, // Es único
                            Estado = "PENDIENTE",
                            FechaRegistro = DateTime.Now,
                            EsFacturable = item.EsFacturable,
                            Activo = true
                        };
                        _context.CuentasPorCobrar.Add(cargoUnico);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Estado de cuenta generado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error generando cargos: {ex.Message}");
            }
        }

        // GET: Ver Estado de Cuenta de un Alumno
        [HttpGet("alumno/{alumnoId}/ciclo/{cicloId}")]
        public async Task<ActionResult<IEnumerable<CuentaPorCobrar>>> GetEstadoCuenta(Guid alumnoId, int cicloId)
        {
            return await _context.CuentasPorCobrar
                .Where(c => c.AlumnoId == alumnoId && c.CicloEscolarId == cicloId)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();
        }
    }
}
