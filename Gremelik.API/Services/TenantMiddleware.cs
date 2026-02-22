using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.EntityFrameworkCore; // <--- Soluciona el error de FirstOrDefaultAsync
using Microsoft.Extensions.DependencyInjection; // <--- Necesario para el Scope

namespace Gremelik.API.Services
{
    public class TenantMiddleware : IMiddleware
    {
        private readonly CurrentTenantService _tenantService;
        // Soluciona el error de "_scopeFactory no existe": La declaramos aquí.
        private readonly IServiceScopeFactory _scopeFactory;

        public TenantMiddleware(CurrentTenantService tenantService, IServiceScopeFactory scopeFactory)
        {
            _tenantService = tenantService;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var tenantSubdominio = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();

            if (!string.IsNullOrEmpty(tenantSubdominio))
            {
                // Muestra en la consola qué está buscando
                Console.WriteLine($"[MIDDLEWARE] 🔍 Buscando escuela con subdominio: '{tenantSubdominio}'");

                _tenantService.Subdominio = tenantSubdominio;

                // Creamos un alcance temporal para consultar la BD
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<GremelikDbContext>();

                    // Buscamos la escuela (ignorando mayúsculas/minúsculas)
                    var escuela = await db.Escuelas
                        .Where(e => e.Subdominio.ToLower() == tenantSubdominio.ToLower())
                        .FirstOrDefaultAsync();

                    if (escuela != null)
                    {
                        // ¡BINGO! Asignamos el ID al servicio
                        _tenantService.TenantId = escuela.Id;
                        Console.WriteLine($"[MIDDLEWARE] ✅ ENCONTRADO! ID: {escuela.Id} asignado al servicio.");
                    }
                    else
                    {
                        // Si sale esto, el texto en BD no coincide con el de la URL
                        Console.WriteLine($"[MIDDLEWARE] ❌ NO SE ENCONTRÓ ninguna escuela con subdominio '{tenantSubdominio}' en la BD.");
                    }
                }
            }
            else
            {
                Console.WriteLine("[MIDDLEWARE] ⚠️ Petición sin Header X-Tenant-ID (Modo Admin o error de Frontend)");
            }

            await next(context);
        }
    }
}
