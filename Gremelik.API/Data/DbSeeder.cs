using Gremelik.core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Gremelik.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Managers de Identity
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Crear Roles si no existen
            string[] roleNames = { "GlobalAdmin", "SchoolAdmin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Asegurarnos que TU usuario sea GlobalAdmin (Opcional, pero útil)
            // Si ya creaste tu usuario "miguel@gremelik.com", esto le asignará el rol automáticamente.
            var adminUser = await userManager.FindByEmailAsync("miguel@gremelik.com");
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, "GlobalAdmin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "GlobalAdmin");
                    // Actualizamos el nombre real si estaba vacío
                    adminUser.NombreCompleto = "Miguel (CEO)";
                    await userManager.UpdateAsync(adminUser);
                }
            }
        }
    }
}
