using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BodegaDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Aplicamos las migraciones pendientes para dejar el esquema al día en PostgreSQL
        await context.Database.MigrateAsync();

        // Creamos un usuario administrador de prueba si no existe
        var adminEmail = "admin@bodega.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "admin123");
        }
    }
}
