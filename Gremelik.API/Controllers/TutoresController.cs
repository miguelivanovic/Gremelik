using Gremelik.core.Entities;
using Gremelik.core.Services; // Necesario para Tenant
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authorization; // Necesario para proteger
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Para leer el usuario logueado

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "GlobalAdmin, SchoolAdmin")] // Agregamos seguridad
    public class TutoresController : ControllerBase
    {
        private readonly GremelikDbContext _context;
        private readonly CurrentTenantService _tenantService; // Agregamos el servicio de Tenant

        // Actualizamos constructor para recibir TenantService
        public TutoresController(GremelikDbContext context, CurrentTenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // GET: api/Tutores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tutor>>> GetTutores()
        {
            // El filtro global en DbContext ya se encarga de filtrar por Escuela
            return await _context.Tutores.ToListAsync();
        }

        // GET: api/Tutores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tutor>> GetTutor(Guid id)
        {
            var tutor = await _context.Tutores.FindAsync(id);
            if (tutor == null) return NotFound();
            return tutor;
        }

        // PUT: api/Tutores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTutor(Guid id, Tutor tutor)
        {
            if (id != tutor.Id) return BadRequest();

            // Aseguramos que no se pierdan datos de auditoría al editar
            // Extrae el nombre o el email del usuario logueado en lugar del ID interno
            var usuarioActual = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "Sistema";
            tutor.Usuario = usuarioActual;
            tutor.FUM = DateTime.Now;

            _context.Entry(tutor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TutorExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/Tutores
        [HttpPost]
        public async Task<ActionResult<Tutor>> PostTutor(Tutor tutor)
        {
            // 1. Asignar Tenant (Escuela)
            if (_tenantService.TenantId.HasValue)
            {
                // Si tu clase Tutor tiene EscuelaId, descomenta esto:
                // tutor.EscuelaId = _tenantService.TenantId.Value;
            }

            // 2. Asignar Datos de Auditoría (CORRECCIÓN DEL ERROR)
            // Obtenemos el usuario real logueado, o usamos "Sistema"
            var usuarioActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            tutor.Usuario = usuarioActual; // <--- ESTO ARREGLA EL ERROR DE REQUIRED
            tutor.FechaRegistro = DateTime.Now;
            tutor.Activo = true;
            tutor.FUM = DateTime.Now;

            _context.Tutores.Add(tutor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTutor", new { id = tutor.Id }, tutor);
        }

        // DELETE: api/Tutores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTutor(Guid id)
        {
            var tutor = await _context.Tutores.FindAsync(id);
            if (tutor == null) return NotFound();

            // REGLA DE ORO: Soft Delete (No borramos, desactivamos)
            tutor.Activo = false;
            tutor.FUM = DateTime.Now;
            tutor.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.Entry(tutor).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{tutorId}/vincular/{alumnoId}")]
        public async Task<IActionResult> VincularTutor(Guid tutorId, Guid alumnoId, [FromBody] string parentesco)
        {
            var relacion = new RelacionAlumnoTutor
            {
                AlumnoId = alumnoId,
                TutorId = tutorId,
                Parentesco = parentesco,
                Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema",
                FechaRegistro = DateTime.Now,
                Activo = true
            };

            _context.Set<RelacionAlumnoTutor>().Add(relacion);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool TutorExists(Guid id)
        {
            return _context.Tutores.Any(e => e.Id == id);
        }
    }


}