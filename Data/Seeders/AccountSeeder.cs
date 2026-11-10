using Microsoft.AspNetCore.Identity;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data.Seeders;

public static class AccountSeeder
{
    private const string DefaultAdminEmail = "admin@bodega.com";

    public static async Task SeedAsync(IServiceProvider services)
    {
        UserManager<User> userManager = services.GetRequiredService<UserManager<User>>();
        RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        IConfiguration configuration = services.GetRequiredService<IConfiguration>();
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(AccountSeeder));

        string adminEmail = configuration["Seed:AdminEmail"] ?? DefaultAdminEmail;
        string? adminPassword = configuration["Seed:AdminPassword"];

        // No hay contraseña por defecto en ningún entorno
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Falta Seed:AdminPassword. Definila en appsettings.Development.json " +
                "o en la variable de entorno Seed__AdminPassword.");
        }

        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            logger.LogInformation("Rol {Role} creado.", Roles.Admin);
        }

        User? admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador",
                EmailConfirmed = true
            };

            IdentityResult created = await userManager.CreateAsync(admin, adminPassword);

            if (!created.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear {adminEmail}: {Describe(created)}");
            }

            logger.LogInformation("Usuario {Email} creado.", adminEmail);
        }
        else if (!await userManager.CheckPasswordAsync(admin, adminPassword))
        {
            // La contraseña configurada manda: si cambió, se actualiza en la base
            string token = await userManager.GeneratePasswordResetTokenAsync(admin);
            IdentityResult reset = await userManager.ResetPasswordAsync(admin, token, adminPassword);

            if (!reset.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo actualizar la contraseña de {adminEmail}: {Describe(reset)}");
            }

            await userManager.ResetAccessFailedCountAsync(admin);
            await userManager.SetLockoutEndDateAsync(admin, null);

            logger.LogInformation("Contraseña de {Email} sincronizada con la configuración.", adminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
            logger.LogInformation("Usuario {Email} asignado al rol {Role}.", adminEmail, Roles.Admin);
        }

        await SeedWarehouseUserAsync(userManager, configuration, logger);
    }

    // Usuario operativo para probar los permisos. Sin las claves de config no se crea
    private static async Task SeedWarehouseUserAsync(
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        string? email = configuration["Seed:WarehouseEmail"];
        string? password = configuration["Seed:WarehousePassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        User? warehouse = await userManager.FindByEmailAsync(email);

        if (warehouse is null)
        {
            warehouse = new User
            {
                UserName = email,
                Email = email,
                FullName = "Operador de almacén",
                EmailConfirmed = true
            };

            IdentityResult created = await userManager.CreateAsync(warehouse, password);

            if (!created.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear {email}: {Describe(created)}");
            }

            logger.LogInformation("Usuario {Email} creado.", email);
        }

        if (!await userManager.IsInRoleAsync(warehouse, Roles.Warehouse))
        {
            await userManager.AddToRoleAsync(warehouse, Roles.Warehouse);
            logger.LogInformation("Usuario {Email} asignado al rol {Role}.", email, Roles.Warehouse);
        }
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
