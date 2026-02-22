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
    public class InscripcionesController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService;

        public InscripcionesController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // --- COTIZAR (Sin cambios, funciona igual) ---
        [HttpGet("cotizar")]
        public async Task<ActionResult<CotizacionInscripcionDto>> Cotizar(int cicloId, int gradoId, bool esNuevoIngreso)
        {
            var resultado = new CotizacionInscripcionDto();

            var grado = await _context.Grados.FindAsync(gradoId);
            if (grado == null) return BadRequest("Grado no válido");

            var costoConfigurado = await _context.CostosInscripcion
                .Where(c => c.CicloEscolarId == cicloId)
                .Where(c => c.GradoId == gradoId || c.NivelEducativoId == grado.NivelEducativoId)
                .OrderByDescending(c => c.GradoId)
                .FirstOrDefaultAsync();

            if (costoConfigurado == null)
            {
                resultado.MontoBase = 0;
                resultado.Concepto = "Precio no configurado";
                return resultado;
            }

            resultado.MontoBase = costoConfigurado.Monto;
            resultado.Concepto = costoConfigurado.Concepto;

            var hoy = DateTime.Today; // FECHA DEL SERVIDOR
            var reglas = await _context.ReglasDescuento
                .Where(r => r.CicloEscolarId == cicloId)
                .ToListAsync();

            var mejorRegla = reglas
                .Where(r => r.Activo)
                .Where(r => (r.FechaInicioValidez == null || r.FechaInicioValidez <= hoy) &&
                            (r.FechaFinValidez == null || r.FechaFinValidez >= hoy))
                .Where(r => r.Tipo == TipoDescuento.FechaLimite ||
                            (esNuevoIngreso && r.Tipo == TipoDescuento.NuevoIngreso) ||
                            (!esNuevoIngreso && r.Tipo == TipoDescuento.Reingreso))
                 .OrderByDescending(r => r.Porcentaje)
                 .ThenByDescending(r => r.MontoFijo)
                 .FirstOrDefault();

            if (mejorRegla != null)
            {
                decimal descuentoPorMonto = mejorRegla.MontoFijo;
                decimal descuentoPorPorcentaje = resultado.MontoBase * (mejorRegla.Porcentaje / 100m);
                resultado.MontoDescuento = descuentoPorMonto + descuentoPorPorcentaje;
                resultado.NombreReglaAplicada = mejorRegla.Nombre;
                resultado.ReglaId = mejorRegla.Id;
            }

            resultado.MontoFinal = resultado.MontoBase - resultado.MontoDescuento;
            if (resultado.MontoFinal < 0) resultado.MontoFinal = 0;

            return resultado;
        }

        public class NuevaInscripcionDto
        {
            public Alumno? Alumno { get; set; }

            // CAMBIO: GrupoId ahora es opcional
            public int? GrupoId { get; set; }

            // CAMBIO: Necesitamos el GradoId por si no hay grupo, saber de qué nivel es
            public int GradoId { get; set; }

            public int CicloId { get; set; }

            public decimal MontoBase { get; set; }
            public decimal MontoDescuento { get; set; }
            public decimal MontoFinal { get; set; }
            public Guid? ReglaId { get; set; }
            public string? MotivoDescuentoManual { get; set; }
        }

        [HttpPost("nuevo-ingreso")]
        public async Task<IActionResult> InscribirNuevoIngreso([FromBody] NuevaInscripcionDto dto)
        {
            if (dto.Alumno == null) return BadRequest("Faltan datos del alumno.");
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada.");

            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Guardar Alumno
                dto.Alumno.EscuelaId = _tenantService.TenantId.Value;
                dto.Alumno.Usuario = usuarioActual;
                dto.Alumno.FechaRegistro = DateTime.Now; // HORA SERVIDOR
                dto.Alumno.Activo = true;

                // --- MATRÍCULA AUTOMÁTICA ---
                string anioActual = DateTime.Now.ToString("yy");
                var ultimaMatricula = await _context.Alumnos
                    .Where(a => a.EscuelaId == _tenantService.TenantId.Value && a.Matricula.StartsWith(anioActual))
                    .OrderByDescending(a => a.Matricula)
                    .Select(a => a.Matricula)
                    .FirstOrDefaultAsync();

                int siguienteConsecutivo = 1;
                if (!string.IsNullOrEmpty(ultimaMatricula) && ultimaMatricula.Length >= 6 && int.TryParse(ultimaMatricula.Substring(2), out int ultimoNumero))
                {
                    siguienteConsecutivo = ultimoNumero + 1;
                }
                dto.Alumno.Matricula = anioActual + siguienteConsecutivo.ToString("D4");

                _context.Alumnos.Add(dto.Alumno);
                await _context.SaveChangesAsync();

                // 2. Determinar Plantel
                Guid plantelId;

                if (dto.GrupoId.HasValue && dto.GrupoId > 0)
                {
                    // Si seleccionó grupo, sacamos el plantel del grupo
                    var grupo = await _context.Grupos
                        .Include(g => g.Grado).ThenInclude(gr => gr.NivelEducativo)
                        .FirstOrDefaultAsync(g => g.Id == dto.GrupoId);

                    if (grupo == null) throw new Exception("Grupo no encontrado");
                    plantelId = grupo.Grado!.NivelEducativo!.PlantelId;
                }
                else
                {
                    // PRE-INSCRIPCIÓN: No hay grupo, usamos el Grado para hallar el plantel
                    var grado = await _context.Grados
                        .Include(g => g.NivelEducativo)
                        .FirstOrDefaultAsync(g => g.Id == dto.GradoId);

                    if (grado == null) throw new Exception("Grado no encontrado");
                    plantelId = grado.NivelEducativo!.PlantelId;
                }

                // 3. Crear Inscripción
                var inscripcion = new Inscripcion
                {
                    Usuario = usuarioActual,
                    AlumnoId = dto.Alumno.Id,
                    GrupoId = (dto.GrupoId > 0) ? dto.GrupoId : null, // Guardamos null si es 0
                    CicloEscolarId = dto.CicloId,
                    PlantelId = plantelId, // Usamos el ID recuperado

                    EsNuevoIngreso = true,
                    FechaRegistro = DateTime.Now, // HORA SERVIDOR
                    Activo = true,
                    Estado = "Inscrito", // Podrías cambiar a "Preinscrito" si grupo es null

                    MontoBase = dto.MontoBase,
                    MontoDescuento = dto.MontoDescuento,
                    MontoFinal = dto.MontoFinal,
                    ReglaDescuentoId = dto.ReglaId,
                    MotivoDescuentoManual = dto.MotivoDescuentoManual
                };

                _context.Inscripciones.Add(inscripcion);
                await _context.SaveChangesAsync();

                // --- NUEVO: GENERAR LA DEUDA EN CAJA ---
                var cuentaInscripcion = new CuentaPorCobrar
                {
                    AlumnoId = dto.Alumno.Id,
                    CicloEscolarId = dto.CicloId,
                    ConceptoNombre = "Inscripción Nuevo Ingreso",
                    FechaVencimiento = DateTime.Today, // Vence el mismo día
                    MontoBase = dto.MontoBase,
                    DescuentoBeca = dto.MontoDescuento,
                    RecargosAcumulados = 0,
                    TotalPagado = 0,
                    Estado = "PENDIENTE",
                    NumeroDePago = 1,
                    EscuelaId = _tenantService.TenantId.Value,
                    EsFacturable = true, // Las inscripciones se facturan
                    Usuario = usuarioActual,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };
                _context.CuentasPorCobrar.Add(cuentaInscripcion);
                await _context.SaveChangesAsync();
                // ----------------------------------------

                await transaction.CommitAsync();
                return Ok(new { mensaje = "Inscripción exitosa", alumnoId = dto.Alumno.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // 1. REINSCRIPCIÓN (Actualizado para guardar GradoId)
        [HttpPost("reinscripcion")]
        public async Task<IActionResult> ReinscribirAlumno([FromBody] NuevaInscripcionDto dto)
        {
            if (dto.Alumno == null || dto.Alumno.Id == Guid.Empty) return BadRequest("Alumno no válido.");

            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            bool yaInscrito = await _context.Inscripciones.AnyAsync(i => i.AlumnoId == dto.Alumno.Id && i.CicloEscolarId == dto.CicloId);
            if (yaInscrito) return BadRequest("El alumno ya está inscrito en este ciclo escolar.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Guid plantelId;
                if (dto.GrupoId.HasValue && dto.GrupoId > 0)
                {
                    var grupo = await _context.Grupos.Include(g => g.Grado).ThenInclude(gr => gr.NivelEducativo).FirstOrDefaultAsync(g => g.Id == dto.GrupoId);
                    plantelId = grupo!.Grado!.NivelEducativo!.PlantelId;
                }
                else
                {
                    var grado = await _context.Grados.Include(g => g.NivelEducativo).FirstOrDefaultAsync(g => g.Id == dto.GradoId);
                    plantelId = grado!.NivelEducativo!.PlantelId;
                }

                var inscripcion = new Inscripcion
                {
                    Usuario = usuarioActual,
                    AlumnoId = dto.Alumno.Id,
                    GrupoId = (dto.GrupoId > 0) ? dto.GrupoId : null,
                    GradoId = dto.GradoId, // <--- AQUI GUARDAMOS EL GRADO
                    CicloEscolarId = dto.CicloId,
                    PlantelId = plantelId,
                    EsNuevoIngreso = false,
                    FechaRegistro = DateTime.Now,
                    Activo = true,
                    Estado = "Inscrito",
                    MontoBase = dto.MontoBase,
                    MontoDescuento = dto.MontoDescuento,
                    MontoFinal = dto.MontoFinal,
                    ReglaDescuentoId = dto.ReglaId,
                    MotivoDescuentoManual = dto.MotivoDescuentoManual
                };

                _context.Inscripciones.Add(inscripcion);
                await _context.SaveChangesAsync();

                // --- NUEVO: GENERAR LA DEUDA EN CAJA ---
                var cuentaReinscripcion = new CuentaPorCobrar
                {
                    AlumnoId = dto.Alumno.Id,
                    CicloEscolarId = dto.CicloId,
                    ConceptoNombre = "Reinscripción",
                    FechaVencimiento = DateTime.Today,
                    MontoBase = dto.MontoBase,
                    DescuentoBeca = dto.MontoDescuento,
                    RecargosAcumulados = 0,
                    TotalPagado = 0,
                    Estado = "PENDIENTE",
                    NumeroDePago = 1,
                    EscuelaId = plantelId, // Usamos la variable que ya tienes ahí
                    EsFacturable = true,
                    Usuario = usuarioActual,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };
                _context.CuentasPorCobrar.Add(cuentaReinscripcion);
                await _context.SaveChangesAsync();
                // ----------------------------------------

                await transaction.CommitAsync();
                return Ok(new { mensaje = "Reinscripción exitosa" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("ultima/{alumnoId}")]
        public async Task<ActionResult<object>> GetUltimaInscripcion(Guid alumnoId)
        {
            var ultima = await _context.Inscripciones
                .Include(i => i.CicloEscolar)
                .Include(i => i.Grupo).ThenInclude(g => g.Grado).ThenInclude(gr => gr.NivelEducativo)
                .Where(i => i.AlumnoId == alumnoId)
                .OrderByDescending(i => i.CicloEscolar.FechaInicio)
                .FirstOrDefaultAsync();

            if (ultima == null) return NotFound("Sin historial");

            return new
            {
                CicloNombre = ultima.CicloEscolar.Nombre,
                GradoNombre = ultima.Grupo?.Grado?.Nombre ?? "Sin Grado",
                GradoId = ultima.Grupo?.GradoId ?? 0,
                NivelId = ultima.Grupo?.Grado?.NivelEducativoId ?? Guid.Empty,

                // --- AGREGADO: Necesitamos saber el plantel ---
                PlantelId = ultima.PlantelId,
                // ----------------------------------------------

                GrupoNombre = ultima.Grupo?.Nombre ?? "Sin Grupo",
                NumeroGrado = ultima.Grupo?.Grado?.Numero ?? 0
            };
        }

        // GET: api/Inscripciones/lista?cicloId=1&gradoId=5&grupoId=10
        [HttpGet("lista")]
        public async Task<ActionResult<IEnumerable<Inscripcion>>> GetLista(int cicloId, int gradoId, int? grupoId = null)
        {
            var query = _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Grupo).ThenInclude(g => g.Grado) // Para mostrar nombre del grupo
                .Where(i => i.CicloEscolarId == cicloId)
                .Where(i => i.Activo); // Solo activos

            if (grupoId.HasValue && grupoId.Value != 0)
            {
                // Filtrar por grupo específico
                query = query.Where(i => i.GrupoId == grupoId);
            }
            else
            {
                // Filtrar por todo el grado (un poco más complejo porque la relación es Inscripcion -> Grupo -> Grado)
                // OJO: Si hay alumnos sin grupo (preinscritos), hay que ver cómo los manejamos.
                // Esta query busca alumnos en grupos de ese grado O alumnos sin grupo pero inscritos a ese grado (si guardamos GradoId en algun lado, pero no lo guardamos directo).
                // Por ahora filtramos por los que TIENEN grupo en ese grado.
                query = query.Where(i => i.Grupo != null && i.Grupo.GradoId == gradoId);
            }

            return await query
                .OrderBy(i => i.Alumno.PrimerApellido)
                .ThenBy(i => i.Alumno.Nombre)
                .ToListAsync();
        }

        // 2. VERIFICAR (Actualizado para leer GradoId)
        [HttpGet("verificar")]
        public async Task<ActionResult<object>> VerificarInscripcion(Guid alumnoId, int cicloId)
        {
            var inscripcion = await _context.Inscripciones
                .Include(i => i.Grupo)
                .Include(i => i.Grado) // Incluimos el Grado directo
                .FirstOrDefaultAsync(i => i.AlumnoId == alumnoId && i.CicloEscolarId == cicloId);

            if (inscripcion != null)
            {
                // Lógica inteligente: Si tiene grupo, usa el del grupo. Si no, usa el Grado directo.
                string nombreGrado = inscripcion.Grupo?.Grado?.Nombre
                                     ?? inscripcion.Grado?.Nombre
                                     ?? "Grado No Especificado";

                return Ok(new
                {
                    yaInscrito = true,
                    fecha = inscripcion.FechaRegistro,
                    grupo = inscripcion.Grupo?.Nombre ?? "Sin Grupo (Pre-inscrito)",
                    grado = nombreGrado, // <--- AHORA SÍ SALDRÁ EL NOMBRE
                    monto = inscripcion.MontoFinal
                });
            }

            return Ok(new { yaInscrito = false });
        }

        // 3. REPORTE AVANCE CON FILTROS
        [HttpGet("avance-reinscripcion")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvanceReinscripcion(
            int cicloActualId,
            int cicloProximoId,
            Guid? plantelId = null,
            Guid? nivelId = null,
            int? gradoId = null,
            int? grupoId = null)
        {
            // 1. Query Base: Alumnos en el ciclo ACTUAL
            var queryActuales = _context.Inscripciones
                .Include(i => i.Alumno)
                .Include(i => i.Grupo).ThenInclude(g => g.Grado)
                .Where(i => i.CicloEscolarId == cicloActualId && i.Activo);

            // 2. Aplicar Filtros (Sobre la ubicación ACTUAL del alumno)
            if (plantelId.HasValue) queryActuales = queryActuales.Where(i => i.PlantelId == plantelId);
            // Nota: Filtramos por Grupo.GradoId porque es donde están ahorita
            if (gradoId.HasValue && gradoId > 0) queryActuales = queryActuales.Where(i => i.Grupo!.GradoId == gradoId);
            if (grupoId.HasValue && grupoId > 0) queryActuales = queryActuales.Where(i => i.GrupoId == grupoId);

            var alumnosActuales = await queryActuales.ToListAsync();

            // 3. Obtener quiénes YA están en el próximo (traemos el objeto completo para ver a dónde van)
            var inscripcionesProximas = await _context.Inscripciones
                .Include(i => i.Grado) // Incluimos Grado destino
                .Include(i => i.Grupo) // Incluimos Grupo destino
                .Where(i => i.CicloEscolarId == cicloProximoId && i.Activo)
                .ToListAsync();

            // Diccionario para búsqueda rápida
            var dictProximos = inscripcionesProximas.ToDictionary(x => x.AlumnoId);

            // 4. Cruzar información
            var reporte = alumnosActuales.Select(i => {
                bool ya = dictProximos.ContainsKey(i.AlumnoId);
                string destino = "-";

                if (ya)
                {
                    var ins = dictProximos[i.AlumnoId];
                    var g = ins.Grupo?.Nombre ?? "Sin Grupo";
                    var gr = ins.Grupo?.Grado?.Nombre ?? ins.Grado?.Nombre ?? "Grado Desc.";
                    destino = $"{gr} ({g})";
                }

                return new
                {
                    Matricula = i.Alumno!.Matricula,
                    NombreCompleto = $"{i.Alumno.PrimerApellido} {i.Alumno.SegundoApellido} {i.Alumno.Nombre}",
                    GradoActual = i.Grupo?.Grado?.Nombre ?? "Sin Grado",
                    GrupoActual = i.Grupo?.Nombre ?? "-",
                    YaSeReinscribio = ya,
                    Destino = destino // <--- Columna nueva para ver a dónde van
                };
            })
            .OrderBy(x => x.YaSeReinscribio)
            .ThenBy(x => x.GradoActual)
            .ToList();

            return Ok(reporte);
        }
    }


    // DTO de Cotización se mantiene igual
    public class CotizacionInscripcionDto
    {
        public decimal MontoBase { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal MontoFinal { get; set; }
        public string Concepto { get; set; } = "";
        public string? NombreReglaAplicada { get; set; }
        public Guid? ReglaId { get; set; }
    }
}