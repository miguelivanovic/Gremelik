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

// 1. Configuración de CORS (Permitir que Blazor hable con la API)
builder.Services.AddCors(options => {
    options.AddPolicy("PermitirBlazor", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 2. Configuración de Base de Datos
builder.Services.AddDbContext<GremelikDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. CONFIGURACIÓN DE IDENTITY (Usuarios y Roles)
// Esto conecta tus tablas de usuarios con la lógica de seguridad
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Relajamos las reglas de contraseña para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<GremelikDbContext>()
.AddDefaultTokenProviders();

// 4. CONFIGURACIÓN DE JWT (El Pasaporte Digital)
// Aquí le decimos cómo validar el token que envía el Frontend
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

// 5. Configuración de Controladores y JSON
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 6. Swagger (Documentación)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Tus Servicios de Tenancy (Multitenencia)
builder.Services.AddScoped<CurrentTenantService>();
builder.Services.AddTransient<TenantMiddleware>();

var app = builder.Build();

/*// --- INICIO: SEMBRADOR DE DATOS ---
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
// --- FIN: SEMBRADOR DE DATOS ---*/

// --- PIPELINE DE PETICIONES (El orden importa mucho aquí) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirBlazor");

app.UseHttpsRedirection(); // Recomendado tenerlo

// IMPORTANTE: Primero Autenticación (¿Quién eres?), luego Autorización (¿Qué puedes hacer?)
app.UseAuthentication();
app.UseAuthorization();

// El Middleware de Tenant va aquí para interceptar el Header X-Tenant-ID
app.UseMiddleware<TenantMiddleware>();

app.UseStaticFiles(); // ESTO ES VITAL: Permite que el navegador pueda ver las imágenes guardadas

app.MapControllers();

app.Run();