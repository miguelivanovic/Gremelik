using System.Text;
using System.Text.Json.Serialization;
using Gremelik.API.Services;
using Gremelik.core.Entities; // Para ApplicationUser
using Gremelik.core.Services;
using Gremelik.data.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Para JWT
using Microsoft.AspNetCore.Identity; // Para Identity
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Para validar el Token


var builder = WebApplication.CreateBuilder(args);

// 1. Configuraci�n de CORS (Permitir que Blazor hable con la API)
builder.Services.AddCors(options => {
    options.AddPolicy("PermitirBlazor", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 2. Configuraci�n de Base de Datos
builder.Services.AddDbContext<GremelikDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. CONFIGURACI�N DE IDENTITY (Usuarios y Roles)
// Esto conecta tus tablas de usuarios con la l�gica de seguridad
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Relajamos las reglas de contrase�a para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<GremelikDbContext>()
.AddDefaultTokenProviders();

// 4. CONFIGURACI�N DE JWT (El Pasaporte Digital)
// Aqu� le decimos c�mo validar el token que env�a el Frontend
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Leemos los valores del appsettings.json
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 5. Configuraci�n de Controladores y JSON
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 6. Swagger (Documentaci�n)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Tus Servicios de Tenancy (Multitenencia)
builder.Services.AddScoped<CurrentTenantService>();
builder.Services.AddTransient<TenantMiddleware>();

builder.Services.AddScoped<Gremelik.API.Services.ProcesadorPagosService>();

builder.Services.AddScoped<CalculadoraDeudasService>();

var app = builder.Build();

// --- INICIO: SEMBRADOR DE DATOS ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ejecutamos la siembra de roles
        await Gremelik.API.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al sembrar datos: " + ex.Message);
    }
}
// --- FIN: SEMBRADOR DE DATOS ---

// --- PIPELINE DE PETICIONES (El orden importa mucho aqu�) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirBlazor");

app.UseHttpsRedirection(); // Recomendado tenerlo

// IMPORTANTE: Primero Autenticaci�n (�Qui�n eres?), luego Autorizaci�n (�Qu� puedes hacer?)
app.UseAuthentication();
app.UseAuthorization();

// El Middleware de Tenant va aqu� para interceptar el Header X-Tenant-ID
app.UseMiddleware<TenantMiddleware>();

app.UseStaticFiles(); // ESTO ES VITAL: Permite que el navegador pueda ver las im�genes guardadas

app.MapControllers();

app.Run();