using Gremelik.core.Entities;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gremelik.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // ANTES: [Authorize]  <-- Esto dejaba pasar a cualquiera logueado
    // AHORA:
    [Authorize(Roles = "GlobalAdmin")] // <-- ¡SOLO TÚ PUEDES PASAR!
    public class EscuelasController : ControllerBase
    {
        private readonly GremelikDbContext _context;

        public EscuelasController(GremelikDbContext context)
        {
            _context = context;
        }

        // GET: api/Escuelas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Escuela>>> GetEscuelas()
        {
            return await _context.Escuelas.ToListAsync();
        }

        // GET: api/Escuelas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Escuela>> GetEscuela(Guid id)
        {
            var escuela = await _context.Escuelas.FindAsync(id);

            if (escuela == null)
            {
                return NotFound();
            }

            return escuela;
        }

       

        // POST: api/Escuelas
        [HttpPost]
        public async Task<ActionResult<Escuela>> PostEscuela(Escuela escuela)
        {
            escuela.FUM = DateTime.Now;
            if (string.IsNullOrEmpty(escuela.Usuario))
            {
                escuela.Usuario = "Sistema";
            }
            // 👇 ¡ESTA ES LA LÍNEA MÁGICA QUE FALTABA! 👇
            escuela.Activo = true;
            escuela.FechaRegistro = DateTime.Now;
            _context.Escuelas.Add(escuela);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEscuela", new { id = escuela.Id }, escuela);
        }

        // PUT: api/Escuelas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEscuela(Guid id, Escuela escuela)
        {
            if (id != escuela.Id) return BadRequest();

            // Protegemos la auditoría
            escuela.FUM = DateTime.Now;
            escuela.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.Entry(escuela).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EscuelaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Escuelas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEscuela(Guid id)
        {
            var escuela = await _context.Escuelas.FindAsync(id);
            if (escuela == null) return NotFound();

            // SOFT DELETE: Nunca borramos, solo apagamos.
            escuela.Activo = false;
            escuela.FUM = DateTime.Now;
            escuela.Usuario = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Sistema";

            _context.Entry(escuela).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Escuelas/upload
        [HttpPost("upload")]
        [AllowAnonymous] // Permitimos subir imágenes temporalmente para facilitar el proceso
        public async Task<IActionResult> UploadImagen(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Archivo vacío");

            // Creamos la ruta física: wwwroot/uploads/escuelas
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "escuelas");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Generamos un nombre único para que no se sobreescriban
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Guardamos el archivo en el disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Devolvemos la URL relativa
            var url = $"/uploads/escuelas/{uniqueFileName}";
            return Ok(new { url });
        }

        private bool EscuelaExists(Guid id)
        {
            return _context.Escuelas.Any(e => e.Id == id);
        }
    }
}