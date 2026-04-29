using Gremelik.core.Entities;
using Gremelik.core.DTOs; // Asegúrate de tener esta carpeta
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador, Maestro")]
    public class AsistenciasController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public AsistenciasController(GremelikDbContext context)
        {
            _context = context;
        }

        // 1. MIS CLASES
        [HttpGet("mis-clases/{cicloId}")]
        public async Task<ActionResult> GetMisClases(int cicloId)
        {
            var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maestroId)) return Unauthorized("Usuario no identificado.");

            var misClases = await _context.AsignacionesMaestros
                .Include(a => a.Materia)
                .Include(a => a.Grupo).ThenInclude(g => g.Grado)
                .Where(a => a.MaestroId == maestroId && a.CicloEscolarId == cicloId && a.Activo)
                .Select(a => new
                {
                    AsignacionId = a.Id,
                    MateriaId = a.MateriaId,
                    MateriaNombre = a.Materia!.Nombre,
                    GrupoId = a.GrupoId,
                    GrupoNombre = $"{a.Grupo!.Grado!.Nombre} {a.Grupo.Nombre}"
                })
                .ToListAsync();

            return Ok(misClases);
        }

        // 2. OBTENER LISTA Y SABER SI YA SE PASÓ
        [HttpGet("lista")]
        public async Task<ActionResult> GetListaAlumnos([FromQuery] int grupoId, [FromQuery] Guid materiaId, [FromQuery] DateTime fecha)
        {
            var inscripciones = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Where(i => i.GrupoId == grupoId && i.Activo)
                .OrderBy(i => i.Alumno!.PrimerApellido).ThenBy(i => i.Alumno!.Nombre)
                .ToListAsync();

            // Verificamos en la cabecera si ya pasaron lista hoy
            bool yaSePasoLista = await _context.BitacorasAsistencia
                .AnyAsync(b => b.GrupoId == grupoId && b.MateriaId == materiaId && b.Fecha.Date == fecha.Date && b.Activo);

            // Buscamos solo las excepciones (faltas, retardos)
            var excepcionesHoy = await _context.Asistencias
                .Where(a => a.GrupoId == grupoId && a.MateriaId == materiaId && a.Fecha.Date == fecha.Date && a.Activo)
                .ToListAsync();

            var listaAlumnos = new List<object>();

            foreach (var ins in inscripciones)
            {
                var excepcion = excepcionesHoy.FirstOrDefault(a => a.AlumnoId == ins.AlumnoId);
                listaAlumnos.Add(new
                {
                    AlumnoId = ins.AlumnoId,
                    NombreCompleto = $"{ins.Alumno!.PrimerApellido} {ins.Alumno.SegundoApellido} {ins.Alumno.Nombre}".Trim(),
                    Matricula = ins.Alumno.Matricula,
                    // Si no hay excepción activa, es Presente (1)
                    Estatus = excepcion != null ? (int)excepcion.Estatus : 1
                });
            }

            // Devolvemos la bandera y la lista juntas
            return Ok(new { YaSePasoLista = yaSePasoLista, Alumnos = listaAlumnos });
        }

        // 3. GUARDAR OPTIMIZADO (ASISTENCIA POR EXCEPCIÓN)
        [HttpPost("guardar")]
        public async Task<IActionResult> GuardarAsistencia([FromBody] GuardarAsistenciaDto dto)
        {
            // CANDADO DE FECHA FUTURA
            if (dto.Fecha.Date > DateTime.Today)
            {
                return BadRequest("Violación de seguridad: No se puede registrar asistencia en el futuro.");
            }

            var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var materiasAImpactar = new List<Guid> { dto.MateriaId };

                if (dto.ReplicarEnOtrasMaterias)
                {
                    var otrasMaterias = await _context.AsignacionesMaestros
                        .Where(a => a.MaestroId == maestroId && a.GrupoId == dto.GrupoId && a.CicloEscolarId == dto.CicloId && a.MateriaId != dto.MateriaId && a.Activo)
                        .Select(a => a.MateriaId)
                        .ToListAsync();
                    materiasAImpactar.AddRange(otrasMaterias);
                }

                foreach (var materiaId in materiasAImpactar)
                {
                    // 1. Registrar la Bitácora (Cabecera)
                    var bitacora = await _context.BitacorasAsistencia
                        .FirstOrDefaultAsync(b => b.GrupoId == dto.GrupoId && b.MateriaId == materiaId && b.Fecha.Date == dto.Fecha.Date && b.Activo);

                    if (bitacora == null)
                    {
                        _context.BitacorasAsistencia.Add(new BitacoraAsistencia { GrupoId = dto.GrupoId, MateriaId = materiaId, Fecha = dto.Fecha.Date, MaestroId = maestroId, Usuario = maestroId, FechaRegistro = DateTime.Now, Activo = true });
                    }

                    // 2. Procesar el Detalle (Excepciones)
                    var excepcionesExistentes = await _context.Asistencias
                        .Where(a => a.GrupoId == dto.GrupoId && a.MateriaId == materiaId && a.Fecha.Date == dto.Fecha.Date && a.Activo)
                        .ToListAsync();

                    foreach (var alumnoInfo in dto.Alumnos)
                    {
                        var registroPrevio = excepcionesExistentes.FirstOrDefault(a => a.AlumnoId == alumnoInfo.AlumnoId);

                        if (alumnoInfo.Estatus == 1) // Es PRESENTE
                        {
                            if (registroPrevio != null)
                            {
                                // El maestro le quitó la falta. Borrado lógico de la excepción.
                                registroPrevio.Activo = false;
                                registroPrevio.FUM = DateTime.Now;
                                _context.Asistencias.Update(registroPrevio);
                            }
                            // Si es presente y no había registro, no hacemos NADA. Ahorramos espacio.
                        }
                        else // ES FALTA, RETARDO O JUSTIFICADO
                        {
                            if (registroPrevio != null)
                            {
                                // Actualizamos el tipo de falta (ej. de Falta a Retardo)
                                registroPrevio.Estatus = (EstatusAsistencia)alumnoInfo.Estatus;
                                registroPrevio.FUM = DateTime.Now;
                                _context.Asistencias.Update(registroPrevio);
                            }
                            else
                            {
                                // Insertamos la nueva excepción
                                // Insertamos la nueva excepción (¡Ahora incluyendo el CicloId!)
                                _context.Asistencias.Add(new Asistencia
                                {
                                    AlumnoId = alumnoInfo.AlumnoId,
                                    GrupoId = dto.GrupoId,
                                    MateriaId = materiaId,
                                    CicloEscolarId = dto.CicloId, // <-- AQUÍ ESTÁ LA MAGIA
                                    Fecha = dto.Fecha.Date,
                                    Estatus = (EstatusAsistencia)alumnoInfo.Estatus,
                                    Usuario = maestroId,
                                    FechaRegistro = DateTime.Now,
                                    Activo = true
                                });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Asistencia guardada correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error interno: " + ex.Message);
            }
        }

        
        // --- 1. LLENAR COMBO DE MATERIAS (Seguro por Rol) ---
        [HttpGet("materias-filtro")]
        public async Task<ActionResult> GetMateriasFiltro([FromQuery] int grupoId, [FromQuery] int cicloId)
        {
            var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool esMaestro = User.IsInRole("Maestro") && !User.IsInRole("GlobalAdmin") && !User.IsInRole("Coordinador") && !User.IsInRole("SchoolAdmin");

            var query = _context.AsignacionesMaestros
                .Include(a => a.Materia)
                .Where(a => a.GrupoId == grupoId && a.CicloEscolarId == cicloId && a.Activo);

            // Si es maestro, solo le devolvemos las materias que él imparte
            if (esMaestro) query = query.Where(a => a.MaestroId == maestroId);

            var materias = await query.Select(a => new { a.MateriaId, Nombre = a.Materia!.Nombre })
                                      .Distinct().ToListAsync();
            return Ok(materias);
        }

        // --- 2. LLENAR COMBO DE ALUMNOS ---
        [HttpGet("alumnos-filtro")]
        public async Task<ActionResult> GetAlumnosFiltro([FromQuery] int grupoId)
        {
            var alumnos = await _context.Inscripciones
                .Include(i => i.Alumno)
                .Where(i => i.GrupoId == grupoId && i.Activo)
                .OrderBy(i => i.Alumno!.PrimerApellido).ThenBy(i => i.Alumno!.Nombre)
                .Select(i => new { i.AlumnoId, NombreCompleto = $"{i.Alumno!.PrimerApellido} {i.Alumno.SegundoApellido} {i.Alumno.Nombre}".Trim() })
                .ToListAsync();
            return Ok(alumnos);
        }

        // --- 3. EL REPORTE (Con Filtros y Seguridad) ---
        // 4. REPORTE DE ASISTENCIAS (MATRIZ)
        [HttpGet("reporte")]
        [Authorize(Roles = "GlobalAdmin, SchoolAdmin, Coordinador, Maestro")]
        public async Task<ActionResult> GetReporteAsistencia([FromQuery] int grupoId, [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] Guid? materiaId = null, [FromQuery] Guid? alumnoId = null, [FromQuery] int cicloId = 0)
        {
            if ((fechaFin - fechaInicio).TotalDays > 60) return BadRequest("El rango no puede ser mayor a 60 días.");

            var maestroId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool esMaestro = User.IsInRole("Maestro") && !User.IsInRole("GlobalAdmin") && !User.IsInRole("Coordinador") && !User.IsInRole("SchoolAdmin");

            // Lista de materias a las que tiene acceso el usuario actual
            List<Guid> materiasPermitidas = new();
            if (esMaestro)
            {
                materiasPermitidas = await _context.AsignacionesMaestros
                    .Where(a => a.MaestroId == maestroId && a.GrupoId == grupoId && a.CicloEscolarId == cicloId && a.Activo)
                    .Select(a => a.MateriaId).ToListAsync();
                if (!materiasPermitidas.Any())
                {
                    return BadRequest("No tienes permisos para ver el reporte de un grupo que no tienes asignado.");
                }
            }

            // 1. Alumnos (Aplicando filtro si lo piden)
            var qAlumnos = _context.Inscripciones.Include(i => i.Alumno).Where(i => i.GrupoId == grupoId && i.Activo);
            if (alumnoId.HasValue && alumnoId.Value != Guid.Empty) qAlumnos = qAlumnos.Where(i => i.AlumnoId == alumnoId.Value);

            var alumnos = await qAlumnos.OrderBy(i => i.Alumno!.PrimerApellido).ThenBy(i => i.Alumno!.Nombre)
                .Select(i => new { i.AlumnoId, NombreCompleto = $"{i.Alumno!.PrimerApellido} {i.Alumno.SegundoApellido} {i.Alumno.Nombre}".Trim() }).ToListAsync();

            // 2. Faltas (Aplicando seguridad y filtros)
            var qExcepciones = _context.Asistencias.Include(a => a.Materia)
                .Where(a => a.GrupoId == grupoId && a.Fecha.Date >= fechaInicio.Date && a.Fecha.Date <= fechaFin.Date && a.Activo);

            if (esMaestro) qExcepciones = qExcepciones.Where(a => materiasPermitidas.Contains(a.MateriaId));
            if (materiaId.HasValue && materiaId.Value != Guid.Empty) qExcepciones = qExcepciones.Where(a => a.MateriaId == materiaId.Value);

            var excepciones = await qExcepciones.ToListAsync();

            // 3. Cabecera de Días (Para saber el total exacto de clases según los filtros)
            var qDias = _context.BitacorasAsistencia
                .Where(b => b.GrupoId == grupoId && b.Fecha.Date >= fechaInicio.Date && b.Fecha.Date <= fechaFin.Date && b.Activo);

            if (esMaestro) qDias = qDias.Where(b => materiasPermitidas.Contains(b.MateriaId));
            if (materiaId.HasValue && materiaId.Value != Guid.Empty) qDias = qDias.Where(b => b.MateriaId == materiaId.Value);

            var diasPasados = await qDias.Select(b => b.Fecha.Date).Distinct().ToListAsync();

            return Ok(new
            {
                DiasConClase = diasPasados,
                Alumnos = alumnos,
                Faltas = excepciones.Select(e => new { e.AlumnoId, Fecha = e.Fecha.Date, Estatus = (int)e.Estatus, MateriaNombre = e.Materia?.Nombre ?? "" })
            });
        }
    }

    // CLASES DTO AUXILIARES PARA RECIBIR LOS DATOS
    public class GuardarAsistenciaDto
    {
        public int GrupoId { get; set; }
        public Guid MateriaId { get; set; }
        public int CicloId { get; set; }
        public DateTime Fecha { get; set; }
        public bool ReplicarEnOtrasMaterias { get; set; }
        public List<RegistroAlumnoDto> Alumnos { get; set; } = new();
    }

    public class RegistroAlumnoDto
    {
        public Guid AlumnoId { get; set; }
        public int Estatus { get; set; }
    }
}