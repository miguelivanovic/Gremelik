using System.Net.Http.Headers;

namespace Gremelik.Web.Services
{
    // Esta clase intercepta TODAS las llamadas HTTP que hace Blazor
    public class TenantHeaderHandler : DelegatingHandler
    {
        private readonly TenantService _tenantService;

        public TenantHeaderHandler(TenantService tenantService)
        {
            _tenantService = tenantService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Averiguamos quién somos (Admin o Escuela X)
            var subdominio = _tenantService.SubdominioActual;

            // 2. Si somos una escuela, le pegamos la etiqueta a la petición
            if (!string.IsNullOrEmpty(subdominio))
            {
                // El header se llamará "X-Tenant-ID"
                request.Headers.Add("X-Tenant-ID", subdominio);
            }

            // 3. Dejamos pasar la petición hacia la API
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
