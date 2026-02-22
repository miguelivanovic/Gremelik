using Microsoft.AspNetCore.Identity;
using System;

namespace Gremelik.core.Entities
{
    // Heredamos de IdentityUser para tener ya todo listo (Password hash, email, teléfono...)
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;

        // AQUÍ ESTÁ LA CLAVE DEL SAAS:
        // Si es NULL = Es un Super Admin (Dueño de Gremelik).
        // Si tiene ID = Es un usuario de esa escuela específica.
        public Guid? EscuelaId { get; set; }
    }
}
