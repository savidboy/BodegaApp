using Microsoft.AspNetCore.Identity;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BodegaDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        // Aseguramos que la base de datos esté creada
        await context.Database.EnsureCreatedAsync();

        // Creamos un usuario administrador de prueba si no existe
        var adminEmail = "admin@bodega.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                NombreCompleto = "Administrador",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "admin123");
        }
    }
}
