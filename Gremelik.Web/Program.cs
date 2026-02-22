using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Gremelik.Web;
using Gremelik.Web.Services;
using Blazored.LocalStorage; // <--- Instalado previamente
using Microsoft.AspNetCore.Components.Authorization; // <--- La librería que acabas de instalar

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Configuración de Servicios Básicos
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5267/") }); // OJO: Verifica tu puerto API aquí
builder.Services.AddScoped<TenantService>();
builder.Services.AddTransient<TenantHeaderHandler>();
builder.Services.AddTransient<JwtInterceptor>(); // <--- NUEVO

// 2. Configuración HTTP Avanzada (Con el Interceptor de Tenant)
// (Aquí habías configurado lo del IHttpClientFactory antes, asegúrate de mantener esa lógica si ya la tenías)
// Si usas la configuración simple de arriba, el interceptor no funcionará bien.
// Te recomiendo volver a poner la configuración del interceptor que hicimos antes aquí:
builder.Services.AddHttpClient("Gremelik.API", client => client.BaseAddress = new Uri("http://localhost:5267/"))
    .AddHttpMessageHandler<TenantHeaderHandler>()
    .AddHttpMessageHandler<JwtInterceptor>();     // Pega el Token de Seguridad
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Gremelik.API"));


// 3. SEGURIDAD (Aquí es donde te daba error antes)
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

// Registramos nuestro proveedor personalizado
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
