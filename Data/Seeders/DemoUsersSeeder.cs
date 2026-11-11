using Microsoft.AspNetCore.Identity;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data.Seeders;

public static class DemoUsersSeeder
{
    private const string DefaultAdminEmail = "admin@bodega.com";
    private const string DefaultWarehouseEmail = "almacen@bodega.com";

    public static async Task SeedAsync(IServiceProvider services)
    {
        UserManager<User> userManager = services.GetRequiredService<UserManager<User>>();
        RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        IConfiguration configuration = services.GetRequiredService<IConfiguration>();
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DemoUsersSeeder));

        string adminEmail = GetConfiguredEmail(configuration, "Seed:AdminEmail", DefaultAdminEmail);
        string adminPassword = GetRequiredPassword(configuration, "Seed:AdminPassword");

        await EnsureRoleExistsAsync(roleManager, Roles.Admin);

        await SeedUserAsync(
            userManager,
            adminEmail,
            adminPassword,
            "Administrador",
            Roles.Admin,
            logger);

        string warehouseEmail = GetConfiguredEmail(configuration, "Seed:WarehouseEmail", DefaultWarehouseEmail);
        string? warehousePassword = configuration["Seed:WarehousePassword"];

        // Sin contraseña configurada no se crea el usuario de almacén
        if (string.IsNullOrWhiteSpace(warehousePassword))
        {
            logger.LogInformation("Sin Seed:WarehousePassword no se crea el usuario de almacén.");
            return;
        }

        if (string.Equals(adminEmail, warehouseEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El administrador y el usuario de almacén deben tener correos diferentes.");
        }

        await EnsureRoleExistsAsync(roleManager, Roles.Warehouse);

        await SeedUserAsync(
            userManager,
            warehouseEmail,
            warehousePassword,
            "Operador de almacén",
            Roles.Warehouse,
            logger);
    }

    private static async Task SeedUserAsync(
        UserManager<User> userManager,
        string email,
        string password,
        string fullName,
        string role,
        ILogger logger)
    {
        User? user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            IdentityResult created = await userManager.CreateAsync(user, password);

            if (!created.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear {email}: {Describe(created)}");
            }

            logger.LogInformation("Usuario demo {Email} creado.", email);
        }
        else if (!await userManager.CheckPasswordAsync(user, password))
        {
            // La contraseña del config manda: si cambió, se actualiza en la base
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult reset = await userManager.ResetPasswordAsync(user, token, password);

            if (!reset.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo actualizar la contraseña de {email}: {Describe(reset)}");
            }

            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);

            logger.LogInformation("Contraseña de {Email} sincronizada con la configuración.", email);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            IdentityResult assigned = await userManager.AddToRoleAsync(user, role);

            if (!assigned.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo asignar el rol {role} a {email}: {Describe(assigned)}");
            }

            logger.LogInformation("Usuario {Email} asignado al rol {Role}.", email, role);
        }
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            throw new InvalidOperationException(
                $"No existe el rol {role}. Ejecuta primero PermissionsSeeder.");
        }
    }

    private static string GetConfiguredEmail(IConfiguration configuration, string key, string defaultEmail)
    {
        string? email = configuration[key];

        return string.IsNullOrWhiteSpace(email) ? defaultEmail : email.Trim();
    }

    private static string GetRequiredPassword(IConfiguration configuration, string key)
    {
        string? password = configuration[key];

        if (string.IsNullOrWhiteSpace(password))
        {
            string environmentVariable = key.Replace(":", "__", StringComparison.Ordinal);

            throw new InvalidOperationException(
                $"Falta {key}. Configúrala en appsettings.Development.json " +
                $"o mediante la variable de entorno {environmentVariable}.");
        }

        return password;
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
