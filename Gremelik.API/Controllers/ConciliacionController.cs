using Gremelik.API.Services;
using Gremelik.core.DTOs;
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
    public class ConciliacionController : ControllerBase
    {
        private readonly CurrentTenantService _tenantService;
        private readonly GremelikDbContext _context;
        private readonly CalculadoraDeudasService _calculadoraDeudas; // <-- EL CEREBRO

        public ConciliacionController(
            CurrentTenantService tenantService,
            GremelikDbContext context,
            CalculadoraDeudasService calculadoraDeudas)
        {
            _tenantService = tenantService;
            _context = context;
            _calculadoraDeudas = calculadoraDeudas;
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<TransaccionPendienteDto>>> GetPendientes()
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            // 1. Obtenemos las transacciones huérfanas
            var huerfanas = await _context.TransaccionesBancarias
                .Where(t => t.EscuelaId == tenantId && t.Estatus == EstatusTransaccion.Huerfana)
                .OrderBy(t => t.FechaPago)
                .ToListAsync();

            // 2. Obtenemos a los alumnos para cruzar datos
            var alumnos = await _context.Alumnos
                .Where(a => a.EscuelaId == tenantId && a.Activo)
                .Select(a => new { a.Id, a.Matricula, a.Nombre, a.PrimerApellido, a.SegundoApellido })
                .ToListAsync();

            // 3. Obtenemos el ciclo escolar actual para el cálculo
            var cicloActual = await _context.CiclosEscolares
                .FirstOrDefaultAsync(c => c.EscuelaId == tenantId && c.Estatus == EstatusCiclo.Actual);

            int cicloId = cicloActual?.Id ?? 0;

            var resultados = new List<TransaccionPendienteDto>();

            foreach (var t in huerfanas)
            {
                var dto = new TransaccionPendienteDto
                {
                    Id = t.Id,
                    FechaPago = t.FechaPago,
                    Monto = t.Monto,
                    ReferenciaBancaria = t.ReferenciaBancaria ?? "",
                    ClaveRastreo = t.ClaveRastreo ?? ""
                };

                var refLower = dto.ReferenciaBancaria.ToLower();
                var rastreoLower = dto.ClaveRastreo.ToLower();

                // PASO A: Buscar si la Matrícula o el Nombre están escondidos en la referencia del banco
                var alumnoSugerido = alumnos.FirstOrDefault(a =>
                    refLower.Contains(a.Matricula.ToLower()) ||
                    rastreoLower.Contains(a.Matricula.ToLower()) ||
                    (refLower.Contains(a.Nombre.ToLower()) && refLower.Contains(a.PrimerApellido.ToLower()))
                );

                if (alumnoSugerido != null && cicloId > 0)
                {
                    // PASO B: Verificamos matemáticamente si el monto tiene sentido (Paquete Completo)
                    try
                    {
                        var deudasAlumno = await _calculadoraDeudas.CalcularDeudasAsync(alumnoSugerido.Id, cicloId, t.FechaPago);

                        // Filtramos solo las que aún deben dinero
                        var deudasPendientes = deudasAlumno.Where(d => (d.MontoBase - d.DescuentoBecaAplicado - d.TotalPagado) > 0).ToList();

                        if (deudasPendientes.Any())
                        {
                            // Buscamos la fecha de vencimiento más antigua
                            var fechaMasAntigua = deudasPendientes.Min(d => d.FechaVencimiento);

                            // Agrupamos TODAS las deudas (Base + Recargos) que vencieron en esa fecha exacta
                            var bloqueMasAntiguo = deudasPendientes.Where(d => d.FechaVencimiento == fechaMasAntigua).ToList();

                            // Calculamos cuánto cuesta ese "paquete" mensual completo y cuánto debe en total
                            decimal totalBloqueAntiguo = bloqueMasAntiguo.Sum(d => d.MontoBase - d.DescuentoBecaAplicado - d.TotalPagado);
                            decimal deudaTotalAcumulada = deudasPendientes.Sum(d => d.MontoBase - d.DescuentoBecaAplicado - d.TotalPagado);

                            // VALIDACIÓN ESTRICTA: El depósito DEBE empatar con el bloque antiguo (Ej. 2400 + 50 = 2450)
                            // O empatar con toda su deuda histórica acumulada.
                            if (totalBloqueAntiguo == t.Monto || deudaTotalAcumulada == t.Monto)
                            {
                                dto.MatriculaSugerida = alumnoSugerido.Matricula;
                                dto.NombreSugerido = $"{alumnoSugerido.Nombre} {alumnoSugerido.PrimerApellido}";
                            }
                        }
                    }
                    catch
                    {
                        // Ignoramos y se va a Huérfanos
                    }
                }

                resultados.Add(dto);
            }

            return Ok(resultados);
        }

        [HttpDelete("descartar/{id}")]
        public async Task<IActionResult> DescartarTransaccion(Guid id)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            var tx = await _context.TransaccionesBancarias
                .FirstOrDefaultAsync(t => t.Id == id && t.EscuelaId == tenantId);

            if (tx == null) return NotFound();

            // Lo eliminamos físicamente del sistema para limpiar la bandeja
            _context.TransaccionesBancarias.Remove(tx);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Transacción descartada" });
        }

        // ... (MANTÉN TU MÉTODO SubirLayoutBanco EXACTAMENTE IGUAL) ...
        // ... (MANTÉN TUS MÉTODOS GetPendientes y Descartar EXACTAMENTE IGUAL) ...



        [HttpGet("deudas-calculadas/{alumnoId}/ciclo/{cicloId}/fecha/{fechaPago}")]
        public async Task<ActionResult<IEnumerable<DeudaCalculadaDto>>> GetDeudasCalculadas(Guid alumnoId, int cicloId, DateTime fechaPago)
        {
            // Le pedimos al cerebro que calcule las deudas AL DÍA EN QUE ENTRÓ EL DINERO AL BANCO
            var resultados = await _calculadoraDeudas.CalcularDeudasAsync(alumnoId, cicloId, fechaPago);
            return Ok(resultados);
        }

        [HttpPost("aprobar-masivas")]
        public async Task<IActionResult> AprobarMasivas([FromBody] List<Guid> transaccionIds)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;
            var usuario = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Sistema";

            var transacciones = await _context.TransaccionesBancarias
                .Where(t => transaccionIds.Contains(t.Id) && t.EscuelaId == tenantId && t.Estatus == EstatusTransaccion.Huerfana)
                .ToListAsync();

            // NUEVO: Traemos a los alumnos a memoria para hacer el mismo "match" de la bandeja visual
            var alumnosEscuela = await _context.Alumnos
                .Where(a => a.EscuelaId == tenantId && a.Activo)
                .ToListAsync();

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            int procesadas = 0;

            try
            {
                foreach (var tx in transacciones)
                {
                    Guid? alumnoIdAUsar = tx.AlumnoId;

                    // Si el depósito es huérfano (null), le re-asignamos el alumno usando la regla visual
                    if (alumnoIdAUsar == null)
                    {
                        var refLower = (tx.ReferenciaBancaria ?? "").ToLower();
                        var rastreoLower = (tx.ClaveRastreo ?? "").ToLower();

                        var alumnoSugerido = alumnosEscuela.FirstOrDefault(a =>
                            refLower.Contains(a.Matricula.ToLower()) ||
                            rastreoLower.Contains(a.Matricula.ToLower()) ||
                            (refLower.Contains(a.Nombre.ToLower()) && refLower.Contains(a.PrimerApellido.ToLower()))
                        );

                        if (alumnoSugerido != null) alumnoIdAUsar = alumnoSugerido.Id;
                    }

                    if (alumnoIdAUsar == null) continue; // Si no se logró identificar, lo salta.

                    var alumno = alumnosEscuela.First(a => a.Id == alumnoIdAUsar.Value);

                    var deudaMasAntigua = await _context.CuentasPorCobrar
                        .Where(c => c.AlumnoId == alumno.Id && c.Activo && c.Estado != "PAGADO")
                        .OrderBy(c => c.FechaVencimiento)
                        .FirstOrDefaultAsync();

                    if (deudaMasAntigua == null) continue;
                    int cicloDelPago = deudaMasAntigua.CicloEscolarId;

                    var deudasCalculadas = await _calculadoraDeudas.CalcularDeudasAsync(alumno.Id, cicloDelPago, tx.FechaPago);

                    // JERARQUÍA DE COBRO (Cronológico: recargos primero, luego base)
                    var deudasOrdenadas = deudasCalculadas
                        .OrderBy(d => d.FechaVencimiento)
                        .ThenByDescending(d => d.EsRecargoVirtual || d.ConceptoNombre.StartsWith("Recargo"))
                        .ToList();

                    decimal montoDisponible = tx.Monto;
                    var detallesNuevos = new List<DetallePago>();
                    bool tieneTutorValido = await _context.RelacionAlumnoTutor.AnyAsync(r => r.AlumnoId == alumno.Id && r.Activo);
                    bool requiereFacturaGlobal = false;

                    foreach (var deudaCalc in deudasOrdenadas)
                    {
                        if (montoDisponible <= 0) break;

                        decimal saldoDeuda = deudaCalc.MontoBase - deudaCalc.DescuentoBecaAplicado - deudaCalc.TotalPagado;
                        if (saldoDeuda <= 0) continue;

                        decimal montoAbonar = Math.Min(montoDisponible, saldoDeuda);
                        montoDisponible -= montoAbonar;

                        Guid cuentaIdFinal = deudaCalc.CuentaPorCobrarId;
                        bool existeEnBD = await _context.CuentasPorCobrar.AnyAsync(c => c.Id == deudaCalc.CuentaPorCobrarId);

                        if (deudaCalc.EsRecargoVirtual && !existeEnBD)
                        {
                            var recargoReal = new CuentaPorCobrar
                            {
                                EscuelaId = tenantId,
                                Usuario = usuario,
                                AlumnoId = alumno.Id,
                                CicloEscolarId = cicloDelPago,
                                ConceptoNombre = deudaCalc.ConceptoNombre,
                                ConceptoPagoId = deudaCalc.ConceptoPagoId,
                                FechaVencimiento = deudaCalc.FechaVencimiento,
                                MontoBase = deudaCalc.MontoBase,
                                DescuentoBeca = 0,
                                TotalPagado = montoAbonar,
                                Estado = montoAbonar >= saldoDeuda ? "PAGADO" : "PARCIAL",
                                EsFacturable = true,
                                Activo = true,
                                FechaRegistro = DateTime.Now,
                                FechaPago = montoAbonar >= saldoDeuda ? tx.FechaPago : null
                            };
                            _context.CuentasPorCobrar.Add(recargoReal);
                            await _context.SaveChangesAsync();
                            cuentaIdFinal = recargoReal.Id;
                        }
                        else
                        {
                            var deudaDb = await _context.CuentasPorCobrar.FindAsync(deudaCalc.CuentaPorCobrarId);
                            if (deudaDb != null)
                            {
                                deudaDb.DescuentoBeca = deudaCalc.DescuentoBecaAplicado;
                                deudaDb.TotalPagado += montoAbonar;

                                decimal saldoRestante = (deudaDb.MontoBase - deudaDb.DescuentoBeca + deudaDb.RecargosAcumulados) - deudaDb.TotalPagado;
                                deudaDb.Estado = saldoRestante <= 0 ? "PAGADO" : "PARCIAL";
                                if (saldoRestante <= 0) deudaDb.FechaPago = tx.FechaPago;
                                deudaDb.FUM = DateTime.Now;
                            }
                        }

                        if (deudaCalc.EsFacturable && tieneTutorValido) requiereFacturaGlobal = true;

                        detallesNuevos.Add(new DetallePago
                        {
                            CuentaPorCobrarId = cuentaIdFinal,
                            MontoAbonado = montoAbonar,
                            ConceptoNombreSnapshot = deudaCalc.ConceptoNombre,
                            Usuario = usuario,
                            FechaRegistro = DateTime.Now,
                            Activo = true
                        });
                    }

                    if (detallesNuevos.Any())
                    {
                        var nuevoPago = new Pago
                        {
                            EscuelaId = tenantId,
                            AlumnoId = alumno.Id,
                            CicloEscolarId = cicloDelPago,
                            FechaPago = tx.FechaPago,
                            TotalPagado = tx.Monto - montoDisponible,
                            DineroRecibido = tx.Monto,
                            MetodoPago = MetodoPago.Transferencia,
                            Comentarios = $"Conciliación Asistida Masiva. Rastreo: {tx.ClaveRastreo}",
                            Usuario = usuario,
                            RequiereFactura = requiereFacturaGlobal,
                            Activo = true,
                            Detalles = detallesNuevos
                        };
                        _context.Pagos.Add(nuevoPago);

                        if (montoDisponible > 0)
                        {
                            alumno.SaldoAFavor += montoDisponible;
                            _context.Alumnos.Update(alumno);
                            nuevoPago.Comentarios += $" | Sobrante de ${montoDisponible:N2} enviado a Saldo a Favor.";
                        }

                        tx.Estatus = EstatusTransaccion.Aplicada;
                        tx.AlumnoId = alumno.Id; // Actualizamos la transacción para enlazarla de por vida
                        tx.PagoGenerado = nuevoPago;
                        procesadas++;
                    }
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                if (procesadas == 0) return BadRequest("No se pudo aplicar ninguna transacción. Verifica que los montos coincidan.");

                return Ok(new { procesadas });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("resolver-manual")]
        public async Task<IActionResult> ResolverManual([FromBody] ResolverConciliacionDto dto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;
            var usuario = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Sistema";

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tx = await _context.TransaccionesBancarias.FirstOrDefaultAsync(t => t.Id == dto.TransaccionId && t.EscuelaId == tenantId);
                if (tx == null || tx.Estatus == EstatusTransaccion.Aplicada) return BadRequest("Transacción inválida o ya aplicada.");

                decimal sumaAbonos = dto.Conceptos.Sum(c => c.MontoAAplicar);
                if (sumaAbonos != tx.Monto) return BadRequest("El monto a repartir no coincide con el depósito.");

                var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
                bool tieneTutorValido = await _context.RelacionAlumnoTutor.AnyAsync(r => r.AlumnoId == alumno!.Id && r.Activo);
                bool requiereFacturaGlobal = false;
                var detallesNuevos = new List<DetallePago>();
                int cicloDelPago = 0;

                foreach (var item in dto.Conceptos)
                {
                    Guid cuentaIdFinal = item.CuentaPorCobrarId;

                    if (item.EsRecargoVirtual)
                    {
                        // CORRECCIÓN: Buscamos la deuda original para heredar su Ciclo Escolar exacto
                        var deudaOrig = await _context.CuentasPorCobrar.FindAsync(item.DeudaOriginalId);
                        int cicloReal = deudaOrig != null ? deudaOrig.CicloEscolarId : 1;
                        if (cicloDelPago == 0) cicloDelPago = cicloReal;

                        var recargoReal = new CuentaPorCobrar
                        {
                            EscuelaId = tenantId,
                            Usuario = usuario,
                            AlumnoId = alumno!.Id,
                            CicloEscolarId = cicloReal, // YA NO ESTÁ FIJO EN 1
                            ConceptoNombre = item.ConceptoNombreVirtual,
                            ConceptoPagoId = item.ConceptoPagoId,
                            FechaVencimiento = deudaOrig != null ? deudaOrig.FechaVencimiento : tx.FechaPago, // Hereda la fecha
                            MontoBase = item.MontoAAplicar, // CORREGIDO: Ahora sí guarda los $50 de base
                            DescuentoBeca = 0,
                            TotalPagado = item.MontoAAplicar,
                            Estado = "PAGADO",
                            EsFacturable = true,
                            Activo = true,
                            FechaRegistro = DateTime.Now,
                            FechaPago = tx.FechaPago
                        };
                        _context.CuentasPorCobrar.Add(recargoReal);
                        await _context.SaveChangesAsync();
                        cuentaIdFinal = recargoReal.Id;
                    }
                    else
                    {
                        var deuda = await _context.CuentasPorCobrar.FindAsync(item.CuentaPorCobrarId);
                        if (cicloDelPago == 0) cicloDelPago = deuda!.CicloEscolarId;

                        deuda!.DescuentoBeca = item.DescuentoBecaFinalCalculado;
                        deuda.TotalPagado += item.MontoAAplicar;
                        decimal nuevoSaldo = (deuda.MontoBase - deuda.DescuentoBeca + deuda.RecargosAcumulados) - deuda.TotalPagado;
                        deuda.Estado = nuevoSaldo <= 0 ? "PAGADO" : "PARCIAL";
                        if (nuevoSaldo <= 0) deuda.FechaPago = tx.FechaPago;
                        if (deuda.EsFacturable && tieneTutorValido) requiereFacturaGlobal = true;
                    }

                    detallesNuevos.Add(new DetallePago
                    {
                        CuentaPorCobrarId = cuentaIdFinal,
                        MontoAbonado = item.MontoAAplicar,
                        ConceptoNombreSnapshot = item.ConceptoNombreVirtual,
                        Usuario = usuario,
                        FechaRegistro = DateTime.Now,
                        Activo = true
                    });
                }

                var nuevoPago = new Pago
                {
                    EscuelaId = tenantId,
                    AlumnoId = alumno!.Id,
                    CicloEscolarId = cicloDelPago > 0 ? cicloDelPago : 1,
                    FechaPago = tx.FechaPago,
                    TotalPagado = tx.Monto,
                    DineroRecibido = tx.Monto,
                    MetodoPago = MetodoPago.Transferencia,
                    Comentarios = $"Conciliación Manual. Rastreo: {tx.ClaveRastreo}",
                    Usuario = usuario,
                    RequiereFactura = requiereFacturaGlobal,
                    Activo = true,
                    Detalles = detallesNuevos
                };

                _context.Pagos.Add(nuevoPago);
                tx.Estatus = EstatusTransaccion.Aplicada;
                tx.AlumnoId = alumno.Id;
                tx.PagoGenerado = nuevoPago;

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return Ok(new { mensaje = "Conciliación manual aplicada." });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }
    }
}