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

        public class GeneracionCargosDto
        {
            public Guid AlumnoId { get; set; }
            public int CicloId { get; set; }
            public Guid? PlanPagoId { get; set; } // <--- AHORA ES OPCIONAL (?)
            public Guid? BecaId { get; set; }
            public List<Guid> ServiciosAdicionalesIds { get; set; } = new();
            public List<Guid> ConceptosUnicosIds { get; set; } = new();
            public DateTime? FechaInicioCobro { get; set; }
        }

        [HttpPost("generar")]
        public async Task<IActionResult> GenerarCargos([FromBody] GeneracionCargosDto dto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var escuelaId = _tenantService.TenantId.Value;
            var usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
            var ciclo = await _context.CiclosEscolares.FindAsync(dto.CicloId);

            // Buscamos el plan SOLO si mandaron un ID válido
            PlanPago? plan = null;
            if (dto.PlanPagoId.HasValue && dto.PlanPagoId.Value != Guid.Empty)
            {
                plan = await _context.PlanesPago.Include(p => p.ConceptoRelacionado).FirstOrDefaultAsync(p => p.Id == dto.PlanPagoId);
            }

            if (alumno == null || ciclo == null) return BadRequest("Datos inválidos.");

            // Validamos que estén intentando generar ALGO
            if (plan == null && !dto.ConceptosUnicosIds.Any() && !dto.ServiciosAdicionalesIds.Any())
            {
                return BadRequest("Debes seleccionar un plan de colegiaturas o al menos un cargo único/servicio.");
            }

            // Validar que si piden servicios mensuales (transporte), ahuevo tengan un plan para saber a cuántos meses
            if (dto.ServiciosAdicionalesIds.Any() && plan == null)
            {
                return BadRequest("Para generar servicios mensuales necesitas asignar un Plan de Colegiaturas primero, ya que de ahí se calcula el número de meses.");
            }

            // 1.5 VALIDACIÓN DE INSCRIPCIÓN EN EL CICLO (La regla que agregamos antes se queda igual)
            var estaInscrito = await _context.Inscripciones.AnyAsync(i => i.AlumnoId == dto.AlumnoId && i.CicloEscolarId == dto.CicloId && i.Activo);
            if (!estaInscrito) return BadRequest("El alumno no está inscrito en este ciclo.");

            // ---------------------------------------------------------
            // 2. VALIDACIÓN DE INSCRIPCIÓN PAGADA
            // ---------------------------------------------------------
            var inscripcionPagada = await _context.CuentasPorCobrar
                .AnyAsync(c => c.AlumnoId == dto.AlumnoId
                            && c.CicloEscolarId == dto.CicloId
                            && c.ConceptoNombre.ToLower().Contains("inscripci")
                            && c.Estado == "PAGADO");

            if (!inscripcionPagada)
            {
                return BadRequest("El alumno debe tener la Inscripción pagada para este ciclo antes de generar colegiaturas.");
            }


            Beca? beca = null;
            decimal descuentoMensual = 0;

            if (dto.BecaId.HasValue)
            {
                beca = await _context.Becas.FindAsync(dto.BecaId);

                // CORRECCIÓN: Validamos que plan NO sea nulo antes de buscar sus propiedades
                if (beca != null && plan != null && plan.ConceptoRelacionado != null && plan.ConceptoRelacionado.AplicaBeca && beca.AplicaEnColegiatura)
                {
                    // ---------------------------------------------------------
                    // 3. VALIDAR REGLAS DE BECA (HERMANOS)
                    // ---------------------------------------------------------
                    if (beca.ReglaHermano != TipoReglaBeca.Ninguna)
                    {
                        // 3.1 Obtener todos los hermanos a través de los tutores
                        var tutoresAlumnoIds = await _context.Set<RelacionAlumnoTutor>()
                            .Where(r => r.AlumnoId == dto.AlumnoId)
                            .Select(r => r.TutorId)
                            .ToListAsync();

                        if (!tutoresAlumnoIds.Any())
                            return BadRequest("No se puede aplicar esta beca: El alumno no tiene tutores asignados.");

                        // Buscar todos los alumnos que comparten esos tutores
                        var hermanosIds = await _context.Set<RelacionAlumnoTutor>()
                            .Where(r => tutoresAlumnoIds.Contains(r.TutorId))
                            .Select(r => r.AlumnoId)
                            .Distinct()
                            .ToListAsync();

                        // Si solo está él, no tiene hermanos
                        if (hermanosIds.Count <= 1)
                            return BadRequest("No se puede aplicar esta beca: El alumno no tiene hermanos registrados en la institución.");

                        // Traer la información completa de los hermanos (incluido el actual)
                        var todosHermanos = await _context.Alumnos
                            .Where(a => hermanosIds.Contains(a.Id) && a.Estatus == EstatusAlumno.Activo)
                            .ToListAsync();

                        // Evaluar la regla seleccionada
                        bool aplicaBeca = false;

                        if (beca.ReglaHermano == TipoReglaBeca.TodosLosHermanos)
                        {
                            aplicaBeca = true; // Ya sabemos que son más de 1
                        }
                        else if (beca.ReglaHermano == TipoReglaBeca.HermanoMayor)
                        {
                            var hermanoMayor = todosHermanos.OrderBy(h => h.FechaNacimiento).First();
                            if (hermanoMayor.Id == dto.AlumnoId) aplicaBeca = true;
                            else return BadRequest($"Esta beca solo aplica al Hermano Mayor (Nacido el {hermanoMayor.FechaNacimiento:d}).");
                        }
                        else if (beca.ReglaHermano == TipoReglaBeca.HermanoMenor)
                        {
                            var hermanoMenor = todosHermanos.OrderByDescending(h => h.FechaNacimiento).First();
                            if (hermanoMenor.Id == dto.AlumnoId) aplicaBeca = true;
                            else return BadRequest($"Esta beca solo aplica al Hermano Menor (Nacido el {hermanoMenor.FechaNacimiento:d}).");
                        }
                        else if (beca.ReglaHermano == TipoReglaBeca.ColegiaturaMasCara || beca.ReglaHermano == TipoReglaBeca.ColegiaturaMasBarata)
                        {
                            var inscripcionesHermanos = await _context.Inscripciones
                                .Include(i => i.Grado)
                                .Where(i => hermanosIds.Contains(i.AlumnoId) && i.CicloEscolarId == dto.CicloId)
                                .ToListAsync();

                            if (beca.ReglaHermano == TipoReglaBeca.ColegiaturaMasCara)
                            {
                                var inscripcionMasCara = inscripcionesHermanos.OrderByDescending(i => i.MontoFinal).FirstOrDefault();
                                if (inscripcionMasCara != null && inscripcionMasCara.AlumnoId == dto.AlumnoId) aplicaBeca = true;
                                else return BadRequest("Esta beca solo aplica al hermano con la colegiatura/plan más caro en este ciclo.");
                            }
                            else if (beca.ReglaHermano == TipoReglaBeca.ColegiaturaMasBarata)
                            {
                                var inscripcionMasBarata = inscripcionesHermanos.OrderBy(i => i.MontoFinal).FirstOrDefault();
                                if (inscripcionMasBarata != null && inscripcionMasBarata.AlumnoId == dto.AlumnoId) aplicaBeca = true;
                                else return BadRequest("Esta beca solo aplica al hermano con la colegiatura/plan más barato en este ciclo.");
                            }
                        }

                        if (!aplicaBeca) return BadRequest("El alumno no cumple con la regla establecida para esta beca de hermanos.");
                    }

                    // ---------------------------------------------------------
                    // 4. CALCULAR DESCUENTO SI PASÓ TODAS LAS REGLAS
                    // ---------------------------------------------------------
                    decimal montoAnual = plan.ConceptoRelacionado.Monto;
                    decimal montoMensual = montoAnual / plan.NumeroPagos;

                    if (beca.Porcentaje > 0) descuentoMensual = montoMensual * (beca.Porcentaje / 100);
                    else descuentoMensual = beca.MontoFijo;
                }
            }


            // Fecha de corte
            DateTime fechaMinimaCobro = dto.FechaInicioCobro ?? ciclo.FechaInicio;
            fechaMinimaCobro = new DateTime(fechaMinimaCobro.Year, fechaMinimaCobro.Month, 1);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ---------------------------------------------------------
                // A) GENERAR COLEGIATURAS (PLAN DE PAGO)
                // ---------------------------------------------------------
                // EL ARREGLO: Primero verificamos que 'plan' no sea nulo
                if (plan != null && plan.ConceptoRelacionado != null)
                {
                    decimal montoAnual = plan.ConceptoRelacionado.Monto;
                    decimal montoMensual = montoAnual / plan.NumeroPagos;

                    var mesesDobles = plan.MesesDobleCobro?
                        .Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int n) ? n : 0)
                        .Where(n => n > 0).ToList() ?? new List<int>();

                    DateTime fechaBase = ciclo.FechaInicio;

                    // ... (El resto del ciclo for se queda exactamente igual) ...

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
                if (dto.ConceptosUnicosIds.Any())
                {
                    // Obtenemos los conceptos que YA se le cobraron en este ciclo para no repetirlos
                    var cargosActualesDelAlumno = await _context.CuentasPorCobrar
                        .Where(c => c.AlumnoId == alumno.Id && c.CicloEscolarId == ciclo.Id && c.Activo)
                        .Select(c => c.ConceptoPagoId)
                        .ToListAsync();

                    var unicos = await _context.ConceptosPago.Where(c => dto.ConceptosUnicosIds.Contains(c.Id)).ToListAsync();

                    foreach (var item in unicos)
                    {
                        // EL FILTRO ANTI-DUPLICIDAD: Si ya tiene este cargo, lo saltamos silenciosamente
                        if (cargosActualesDelAlumno.Contains(item.Id)) continue;

                        DateTime fechaVencimiento = dto.FechaInicioCobro ?? DateTime.Now;

                        var cargoUnico = new CuentaPorCobrar
                        {
                            EscuelaId = escuelaId,
                            Usuario = usuario,
                            AlumnoId = alumno.Id,
                            CicloEscolarId = ciclo.Id,
                            ConceptoNombre = item.Nombre,
                            ConceptoPagoId = item.Id,
                            FechaVencimiento = fechaVencimiento,
                            MontoBase = item.Monto,
                            DescuentoBeca = 0,
                            NumeroDePago = 1,
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