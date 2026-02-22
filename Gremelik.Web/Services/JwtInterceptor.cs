using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace Gremelik.Web.Services
{
    public class JwtInterceptor : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public JwtInterceptor(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Buscamos el token en el navegador
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // 2. Si existe, se lo pegamos a la petición
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 3. Dejamos que la petición continúe su viaje a la API
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
