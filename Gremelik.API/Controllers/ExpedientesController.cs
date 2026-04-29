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
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador")] // Solo perfil gerencial
    public class ExpedientesController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService; // 1. Agregamos esta línea

        // 2. Lo inyectamos en el constructor
        public ExpedientesController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet("360/{alumnoId}/ciclo/{cicloId}")]
        public async Task<ActionResult<Expediente360Dto>> GetExpedienteIntegral(Guid alumnoId, int cicloId)
        {
            // 1. OBTENER ALUMNO
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null) return NotFound("Alumno no encontrado.");

            // 2. OBTENER LA ESCUELA (¡Esto es lo que le faltaba a este reporte!)
            var escuela = await _context.Escuelas.FindAsync(_tenantService.TenantId.Value);

            // 3. OBTENER FICHA MÉDICA
            var ficha = await _context.FichasMedicas.FirstOrDefaultAsync(f => f.AlumnoId == alumnoId && f.Activo);

            var dto = new Expediente360Dto
            {
                // PASAMOS EL LOGO Y NOMBRE DE LA BD AL DTO
                EscuelaNombre = escuela?.Nombre ?? "",
                EscuelaLogoUrl = escuela?.LogoUrl,
                EscuelaSlogan = escuela?.Slogan,

                // Resto de los datos del alumno...
                AlumnoId = alumno.Id,
                Matricula = alumno.Matricula ?? "S/N",
                NombreCompleto = $"{alumno.PrimerApellido} {alumno.SegundoApellido} {alumno.Nombre}".Trim(),
                Curp = alumno.CURP ?? "N/A",
                TipoSangre = ficha?.TipoSangre ?? "No registrado",
                Alergias = ficha?.Alergias ?? "No registradas",
                ContactoEmergencia = ficha != null ? $"{ficha.NombreContactoEmergencia} ({ficha.TelefonoContactoEmergencia})" : "No registrado"
            };

            // 2. OBTENER TUTORES
            var relaciones = await _context.RelacionAlumnoTutor
                .Where(r => r.AlumnoId == alumnoId && r.Activo)
                .ToListAsync(); // Traemos las relaciones primero

            if (relaciones.Any())
            {
                var tutorIds = relaciones.Select(r => r.TutorId).ToList();
                var tutoresDb = await _context.Tutores.Where(t => tutorIds.Contains(t.Id) && t.Activo).ToListAsync();

                foreach (var rel in relaciones)
                {
                    var tutor = tutoresDb.FirstOrDefault(t => t.Id == rel.TutorId);
                    if (tutor != null)
                    {
                        dto.Tutores.Add(new TutorExpedienteDto
                        {
                            NombreCompleto = $"{tutor.Nombre} {tutor.PrimerApellido} {tutor.SegundoApellido}".Trim(),
                            Parentesco = rel.Parentesco,
                            Telefono = "Ver Perfil" // O el campo de teléfono si lo agregas al Tutor después
                        });
                    }
                }
            }

            // 3. FINANZAS (Cuentas por Cobrar del Ciclo)
            var cuentas = await _context.CuentasPorCobrar
                .Where(c => c.AlumnoId == alumnoId && c.CicloEscolarId == cicloId && c.Activo)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            dto.TotalCargos = cuentas.Sum(c => c.MontoBase - c.DescuentoBeca + c.RecargosAcumulados);
            dto.TotalPagado = cuentas.Sum(c => c.TotalPagado);
            dto.SaldoVencido = cuentas.Where(c => c.FechaVencimiento.Date < DateTime.Today && c.Estado != "PAGADO").Sum(c => c.SaldoPendiente);
            dto.PagosAtrasados = cuentas.Count(c => c.FechaVencimiento.Date < DateTime.Today && c.Estado != "PAGADO");

            dto.EstadoDeCuenta = cuentas.Select(c => new CargoExpedienteDto
            {
                Concepto = c.ConceptoNombre,
                Vencimiento = c.FechaVencimiento,
                MontoBase = c.MontoBase,
                SaldoPendiente = c.SaldoPendiente,
                Estado = c.Estado
            }).ToList();

            // 4. ACADÉMICO (Desglose por Periodo Interno Completo)
            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.AlumnoId == alumnoId && i.CicloEscolarId == cicloId && i.Activo);

            if (inscripcion != null)
            {
                var calificaciones = await _context.CalificacionesInternas
                    .Include(c => c.Materia)
                    .Include(c => c.Periodo)
                    .Where(c => c.AlumnoId == alumnoId && c.GrupoId == inscripcion.GrupoId && c.Activo)
                    .ToListAsync();

                if (calificaciones.Any())
                {
                    // Descubrimos a qué nivel educativo pertenece el alumno basándonos en sus materias
                    var nivelEduId = calificaciones.First().Periodo!.NivelEducativoId;

                    // Traemos TODOS los periodos de ese nivel y ciclo, ordenados cronológicamente
                    var todosLosPeriodos = await _context.PeriodosInternos
                        .Where(p => p.CicloEscolarId == cicloId && p.NivelEducativoId == nivelEduId && p.Activo)
                        .OrderBy(p => p.Orden)
                        .ToListAsync();

                    // Guardamos los nombres abreviados a 3 letras (Ej: SEP, OCT, NOV)
                    dto.PeriodosEvaluados = todosLosPeriodos
                        .Select(p => p.Nombre.Substring(0, Math.Min(3, p.Nombre.Length)).ToUpper())
                        .ToList();

                    var materiasIds = calificaciones.Select(c => c.MateriaId).Distinct();
                    foreach (var matId in materiasIds)
                    {
                        var notasMateria = calificaciones.Where(c => c.MateriaId == matId).ToList();
                        var filaBoleta = new CalificacionExpedienteDto
                        {
                            Materia = notasMateria.First().Materia?.Nombre ?? "Desconocida"
                        };

                        // Llenamos las notas iterando sobre TODOS los periodos (si no hay nota, se va en 0)
                        foreach (var periodo in todosLosPeriodos)
                        {
                            var notaExacta = notasMateria.FirstOrDefault(n => n.PeriodoInternoId == periodo.Id)?.Nota ?? 0;
                            filaBoleta.Notas.Add(notaExacta);
                        }

                        // Calculamos el promedio solo con los periodos que sí tienen nota
                        var notasValidas = filaBoleta.Notas.Where(n => n > 0).ToList();
                        filaBoleta.Promedio = notasValidas.Any() ? Math.Round(notasValidas.Average(), 1) : 0;

                        dto.Boleta.Add(filaBoleta);
                    }

                    if (dto.Boleta.Any())
                    {
                        dto.PromedioGeneralSEP = Math.Round(dto.Boleta.Average(b => b.Promedio), 1);
                    }
                }
            }

            // 5. ASISTENCIAS (Total de faltas en el ciclo)
            // Agrupamos por fecha para que 1 día = 1 falta, como lo hicimos en el Dashboard
            dto.TotalFaltas = await _context.Asistencias
                .Where(a => a.AlumnoId == alumnoId && a.CicloEscolarId == cicloId && a.Estatus == EstatusAsistencia.Falta && a.Activo)
                .Select(a => a.Fecha.Date)
                .Distinct()
                .CountAsync();

            // 6. DISCIPLINA (Reportes del ciclo)
            var reportes = await _context.ReportesConducta
                .Where(r => r.AlumnoId == alumnoId && r.CicloEscolarId == cicloId && r.Activo)
                .OrderByDescending(r => r.FechaIncidencia)
                .ToListAsync();

            dto.ReportesConducta = reportes.Select(r => new ReporteExpedienteDto
            {
                Fecha = r.FechaIncidencia,
                Titulo = r.Titulo,
                Gravedad = (int)r.Gravedad,
                Estatus = r.Estatus == EstatusReporte.Pendiente ? "Pendiente" : r.Estatus == EstatusReporte.EnProceso ? "En Proceso" : "Cerrado"
            }).ToList();

            // Retornamos el MEGA JSON
            return Ok(dto);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult> BuscarAlumnosRapido([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());

            var query = q.ToLower();
            var alumnos = await _context.Alumnos
                .Where(a => a.Nombre.ToLower().Contains(query) ||
                            a.PrimerApellido.ToLower().Contains(query) ||
                            (a.Matricula != null && a.Matricula.ToLower().Contains(query)))
                .Take(15) // Top 15 para no saturar la red
                .Select(a => new {
                    AlumnoId = a.Id,
                    NombreCompleto = $"{a.PrimerApellido} {a.SegundoApellido} {a.Nombre} - {a.Matricula}".Trim()
                })
                .ToListAsync();

            return Ok(alumnos);
        }
    }

    public class Expediente360Dto
    {
        // --- BRANDING DE LA ESCUELA ---
        public string EscuelaNombre { get; set; } = "";
        public string? EscuelaLogoUrl { get; set; }
        public string? EscuelaSlogan { get; set; }
        // 1. PERFIL Y CONTACTO
        public Guid AlumnoId { get; set; }
        public string Matricula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Curp { get; set; } = "";

        // Ficha Médica
        public string TipoSangre { get; set; } = "No registrado";
        public string Alergias { get; set; } = "Ninguna";
        public string ContactoEmergencia { get; set; } = "No registrado";

        // Tutores
        public List<TutorExpedienteDto> Tutores { get; set; } = new();

        // 2. FINANZAS (Resumen del Ciclo)
        public decimal TotalCargos { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoVencido { get; set; }
        public int PagosAtrasados { get; set; }
        public List<CargoExpedienteDto> EstadoDeCuenta { get; set; } = new();

        // 3. ACADÉMICO Y OPERATIVO
        public int TotalFaltas { get; set; }
        public decimal PromedioGeneralSEP { get; set; }

        // --- LO NUEVO PARA LA BOLETA DINÁMICA ---
        public List<string> PeriodosEvaluados { get; set; } = new();
        public List<CalificacionExpedienteDto> Boleta { get; set; } = new();

        // 4. DISCIPLINA
        public List<ReporteExpedienteDto> ReportesConducta { get; set; } = new();
    }

    public class CalificacionExpedienteDto
    {
        public string Materia { get; set; } = "";
        public List<decimal> Notas { get; set; } = new(); // Las notas ordenadas por periodo
        public decimal Promedio { get; set; }
    }
    public class TutorExpedienteDto { public string NombreCompleto { get; set; } = ""; public string Parentesco { get; set; } = ""; public string Telefono { get; set; } = ""; }
    public class CargoExpedienteDto { public string Concepto { get; set; } = ""; public DateTime Vencimiento { get; set; } public decimal MontoBase { get; set; } public decimal SaldoPendiente { get; set; } public string Estado { get; set; } = ""; }
    public class ReporteExpedienteDto { public DateTime Fecha { get; set; } public string Titulo { get; set; } = ""; public int Gravedad { get; set; } public string Estatus { get; set; } = ""; }
        
}