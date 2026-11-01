using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data;

public static class DbInitializer
{
    private const string DefaultAdminEmail = "admin@bodega.com";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BodegaDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));

        // Aplicamos las migraciones pendientes para dejar el esquema al día en PostgreSQL
        await context.Database.MigrateAsync();

        // El administrador sale de configuración (Seed:AdminEmail / Seed:AdminPassword)
        var adminEmail = configuration["Seed:AdminEmail"] ?? DefaultAdminEmail;
        var adminPassword = configuration["Seed:AdminPassword"];

        // No hay contraseña por defecto en ningún entorno
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Falta la configuración Seed:AdminPassword. En Docker va en el .env " +
                "(SEED_ADMIN_PASSWORD) y en local en appsettings.Development.json o en la " +
                "variable de entorno Seed__AdminPassword.");
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Creamos un usuario administrador de prueba si no existe
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el administrador {adminEmail}: {Describe(createResult)}");
            }

            logger.LogInformation("Usuario administrador {Email} creado.", adminEmail);
        }
        else if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
        {
            // La contraseña configurada manda: si cambió, se actualiza en la base
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo actualizar la contraseña del administrador {adminEmail}: {Describe(resetResult)}");
            }

            // Si quedó bloqueado por intentos fallidos, se limpia para poder entrar
            await userManager.ResetAccessFailedCountAsync(adminUser);
            await userManager.SetLockoutEndDateAsync(adminUser, null);

            logger.LogInformation("Contraseña del administrador {Email} sincronizada con la configuración.", adminEmail);
        }

        // El admin es el único con permiso para borrar de forma permanente
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            logger.LogInformation("Rol {Role} creado.", Roles.Admin);
        }

        if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            logger.LogInformation("Usuario {Email} asignado al rol {Role}.", adminEmail, Roles.Admin);
        }
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(e => e.Description));
}
