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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")]
    public class CajaController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public CajaController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("tutores/{alumnoId}")]
        public async Task<ActionResult<IEnumerable<Tutor>>> GetTutoresPorAlumno(Guid alumnoId)
        {
            // Buscamos las relaciones de este alumno y cruzamos con la tabla Tutores
            var tutores = await (from r in _context.Set<RelacionAlumnoTutor>()
                                 join t in _context.Set<Tutor>() on r.TutorId equals t.Id
                                 where r.AlumnoId == alumnoId && r.Activo && t.Activo
                                 select t).ToListAsync();

            return Ok(tutores);
        }

        [HttpPost("cobrar")]
        public async Task<IActionResult> Cobrar([FromBody] NuevoPagoDto dto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");

            if (dto.ConceptosAPagar.Count == 0) return BadRequest("No hay conceptos seleccionados para pagar.");

            var escuelaId = _tenantService.TenantId.Value;
            // Extrae el nombre o el email del usuario logueado en lugar del ID interno
            var usuario = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Sistema";
            var totalTicket = dto.ConceptosAPagar.Sum(x => x.MontoAPagar);

            // CORRECCIÓN 1: Convertimos explícitamente (MetodoPago)
            if ((MetodoPago)dto.MetodoPago == MetodoPago.Efectivo && dto.DineroRecibido < totalTicket)
                return BadRequest("El dinero recibido es insuficiente.");

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
                    TotalPagado = totalTicket,
                    // CORRECCIÓN 2: Conversión explícita
                    DineroRecibido = (MetodoPago)dto.MetodoPago == MetodoPago.Efectivo ? dto.DineroRecibido : totalTicket,
                    Cambio = (MetodoPago)dto.MetodoPago == MetodoPago.Efectivo ? (dto.DineroRecibido - totalTicket) : 0,
                    // CORRECCIÓN 3: Conversión explícita
                    MetodoPago = (MetodoPago)dto.MetodoPago,
                    Comentarios = dto.Comentarios,
                    // --- NUEVOS CAMPOS ---
                    RequiereFactura = dto.RequiereFactura,
                    TutorId = dto.RequiereFactura ? dto.TutorId : null, // Solo guardamos el tutor si sí quiere factura
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                _context.Pagos.Add(nuevoPago);
                await _context.SaveChangesAsync();

                foreach (var item in dto.ConceptosAPagar)
                {
                    var deuda = await _context.CuentasPorCobrar.FindAsync(item.CuentaPorCobrarId);
                    if (deuda == null) throw new Exception("Cuenta por cobrar no encontrada");

                    if (item.MontoAPagar > deuda.SaldoPendiente)
                        throw new Exception($"Estás intentando pagar ${item.MontoAPagar} a '{deuda.ConceptoNombre}' pero solo debe ${deuda.SaldoPendiente}");

                    var detalle = new DetallePago
                    {
                        PagoId = nuevoPago.Id,
                        CuentaPorCobrarId = item.CuentaPorCobrarId,
                        MontoAbonado = item.MontoAPagar,
                        ConceptoNombreSnapshot = deuda.ConceptoNombre,
                        Usuario = usuario,
                        FechaRegistro = DateTime.Now,
                        Activo = true
                    };
                    _context.DetallesPagos.Add(detalle);

                    deuda.TotalPagado += item.MontoAPagar;

                    if (deuda.SaldoPendiente < 0.01m) deuda.Estado = "PAGADO";
                    else deuda.Estado = "PARCIAL";

                    deuda.FUM = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Cobro exitoso",
                    folio = nuevoPago.Folio,
                    cambio = nuevoPago.Cambio,
                    pagoId = nuevoPago.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }
    }
}