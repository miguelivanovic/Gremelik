using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gremelik.core.DTOs;
using Gremelik.core.Services;
using System.Security.Claims;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlumnosController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService; // O como se llame tu interfaz

        public AlumnosController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }
        // ...

        // GET: api/Alumnos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alumno>>> GetAlumnos()
        {
            return await _context.Alumnos
                .OrderBy(a => a.PrimerApellido) // Ordenado por apellido por defecto
                .ToListAsync();
        }

        // Arriba, asegúrate de tener: using Gremelik.core.DTOs;

        // 1. EL BUSCADOR ORIGINAL (Para la Caja, Boletas, etc. Devuelve el Alumno completo)
        // 1. EL BUSCADOR ORIGINAL (Para la Caja, Boletas, etc. Devuelve el Alumno completo)
        [HttpGet("buscar/{texto}")]
        public async Task<ActionResult<IEnumerable<Alumno>>> BuscarAlumnos(string texto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            if (string.IsNullOrWhiteSpace(texto)) return new List<Alumno>();

            // 1. Partimos el texto en palabras individuales
            var terminos = texto.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 2. Iniciamos la consulta base
            var query = _context.Alumnos.Where(a => a.EscuelaId == tenantId && a.Activo);

            // 3. MAGIA: Agregamos un filtro obligatorio por cada palabra escrita
            foreach (var termino in terminos)
            {
                query = query.Where(a => a.Nombre.ToLower().Contains(termino) ||
                                         a.PrimerApellido.ToLower().Contains(termino) ||
                                         (a.SegundoApellido != null && a.SegundoApellido.ToLower().Contains(termino)) ||
                                         a.Matricula.ToLower().Contains(termino) ||
                                         a.CURP.ToLower().Contains(termino));
            }

            // 4. Ejecutamos la consulta devolviendo el objeto pesado (hasta 20 resultados)
            var resultados = await query
                .OrderBy(a => a.PrimerApellido).ThenBy(a => a.Nombre)
                .Take(20)
                .ToListAsync();

            return Ok(resultados);
        }

        // 2. EL NUEVO BUSCADOR OPTIMIZADO (Exclusivo para la pantalla de Directorio de Alumnos)
        [HttpGet("directorio/{texto}")]
        public async Task<ActionResult<IEnumerable<AlumnoBusquedaDto>>> BuscarAlumnosDirectorio(string texto)
        {
            if (!_tenantService.TenantId.HasValue) return BadRequest("Escuela no identificada");
            var tenantId = _tenantService.TenantId.Value;

            if (string.IsNullOrWhiteSpace(texto)) return new List<AlumnoBusquedaDto>();

            // 1. Partimos el texto en palabras individuales, ignorando los espacios en blanco extra
            var terminos = texto.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var cicloActualId = await _context.Set<CicloEscolar>()
                .Where(c => c.EscuelaId == tenantId && c.Estatus == EstatusCiclo.Actual)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            // 2. Iniciamos la consulta base
            var query = _context.Alumnos.Where(a => a.EscuelaId == tenantId && a.Activo);

            // 3. MAGIA: Por cada palabra que escribió el usuario, agregamos una condición.
            // Esto asegura que "todas" las palabras coincidan en algún lugar del alumno.
            foreach (var termino in terminos)
            {
                query = query.Where(a => a.Nombre.ToLower().Contains(termino) ||
                                         a.PrimerApellido.ToLower().Contains(termino) ||
                                         (a.SegundoApellido != null && a.SegundoApellido.ToLower().Contains(termino)) ||
                                         a.Matricula.ToLower().Contains(termino) ||
                                         a.CURP.ToLower().Contains(termino));
            }

            var resultados = await query
                .Select(a => new AlumnoBusquedaDto
                {
                    Id = a.Id,
                    Matricula = a.Matricula,
                    NombreCompleto = a.Nombre + " " + a.PrimerApellido + " " + (a.SegundoApellido ?? ""),
                    // ... (El resto de tus campos Select se quedan exactamente igual) ...
                    CURP = a.CURP,
                    NIA = a.NIA,
                    Estatus = a.Estatus,
                    Plantel = a.Inscripciones.Where(i => i.Activo && i.CicloEscolarId == cicloActualId).Select(i => i.Plantel!.Nombre).FirstOrDefault() ?? "Sin Asignar",
                    Nivel = a.Inscripciones.Where(i => i.Activo && i.CicloEscolarId == cicloActualId).Select(i => i.Grado!.NivelEducativo!.Nombre).FirstOrDefault() ?? "-",
                    Grado = a.Inscripciones.Where(i => i.Activo && i.CicloEscolarId == cicloActualId).Select(i => i.Grado!.Nombre).FirstOrDefault() ?? "-",
                    Grupo = a.Inscripciones.Where(i => i.Activo && i.CicloEscolarId == cicloActualId).Select(i => i.Grupo!.Nombre).FirstOrDefault() ?? "-"
                })
                .OrderBy(a => a.NombreCompleto)
                .Take(30)
                .ToListAsync();

            return Ok(resultados);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Alumno>> GetAlumno(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return NotFound();
            return alumno;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlumno(Guid id, Alumno alumno)
        {
            if (id != alumno.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            _context.Entry(alumno).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlumnoExists(id)) return NotFound();
                else throw;
            }
            catch (DbUpdateException)
            {
                if (AlumnoExists(alumno.Id)) return Conflict();
                return BadRequest("Error al actualizar. Verifique datos duplicados.");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Alumno>> PostAlumno(Alumno alumno)
        {
            try
            {
                _context.Alumnos.Add(alumno);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("duplicate") == true ||
                    ex.InnerException?.Message.Contains("UNIQUE") == true)
                {
                    return BadRequest($"Ya existe un alumno con la CURP '{alumno.CURP}' en esta escuela.");
                }
                return BadRequest($"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
            }

            return CreatedAtAction("GetAlumno", new { id = alumno.Id }, alumno);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlumno(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null) return NotFound();

            // REGLA DE ORO: No borramos, inactivamos
            alumno.Activo = false;
            alumno.FUM = DateTime.Now; // Fecha de Última Modificación
            alumno.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.Alumnos.Update(alumno);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool AlumnoExists(Guid id)
        {
            return _context.Alumnos.Any(e => e.Id == id);
        }
    }
}