using Blazored.LocalStorage;
using Gremelik.core.DTOs; // Asegúrate de tener acceso a LoginDto
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace Gremelik.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient,
                           ILocalStorageService localStorage,
                           AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<string?> Login(LoginDto loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginModel);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserSessionDto>();

                // 1. Guardamos el token en el navegador
                await _localStorage.SetItemAsync("authToken", result!.Token);

                // 2. Avisamos a Blazor que el estado cambió (para que actualice menús)
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);

                return null; // Null significa "Sin errores"
            }

            return "Error al iniciar sesión. Verifique sus credenciales.";
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
