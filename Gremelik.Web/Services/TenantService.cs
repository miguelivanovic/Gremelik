using Microsoft.AspNetCore.Components;

namespace Gremelik.Web.Services
{
    public class TenantService
    {
        private readonly NavigationManager _navManager;

        // Aquí guardaremos quién es la escuela actual (si aplica)
        public string? SubdominioActual { get; private set; }
        public bool EsAdmin { get; private set; }

        public TenantService(NavigationManager navManager)
        {
            _navManager = navManager;
            DetectarTenant();
        }

        public void DetectarTenant()
        {
            var uri = _navManager.ToAbsoluteUri(_navManager.Uri);
            var host = uri.Host; // Ejemplo: "admin.gremelik.local"

            // Lógica: Si empieza con "admin" o es "localhost", eres el jefe.
            if (host.StartsWith("admin") || host.StartsWith("localhost"))
            {
                EsAdmin = true;
                SubdominioActual = null;
            }
            else
            {
                // Es una escuela (ej. "prueba.gremelik.local")
                EsAdmin = false;
                // Sacamos la primera parte del dominio
                SubdominioActual = host.Split('.')[0];
            }
        }
    }
}
