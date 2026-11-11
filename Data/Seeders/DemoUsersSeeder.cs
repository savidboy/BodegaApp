
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
        UserManager<User> userManager =
            services.GetRequiredService<UserManager<User>>();

        RoleManager<IdentityRole> roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        IConfiguration configuration =
            services.GetRequiredService<IConfiguration>();

        ILogger logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DemoUsersSeeder));

        string adminEmail = GetConfiguredEmail(
            configuration, "Seed:AdminEmail", DefaultAdminEmail);

        string warehouseEmail = GetConfiguredEmail(
            configuration, "Seed:WarehouseEmail", DefaultWarehouseEmail);

        string adminPassword = GetRequiredPassword(
            configuration, "Seed:AdminPassword");

        string warehousePassword = GetRequiredPassword(
            configuration, "Seed:WarehousePassword");

        if (string.Equals(
            adminEmail, warehouseEmail,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El administrador y el usuario de almacén deben tener correos diferentes.");
        }

        // Los roles deben existir antes de crear los usuarios.
        await EnsureRoleExistsAsync(roleManager, Roles.Admin);
        await EnsureRoleExistsAsync(roleManager, Roles.Warehouse);

        await SeedUserAsync(
            userManager,
            adminEmail,
            adminPassword,
            "Administrador Demo",
            Roles.Admin,
            logger);

        await SeedUserAsync(
            userManager,
            warehouseEmail,
            warehousePassword,
            "Operador de Almacén Demo",
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

            IdentityResult created =
                await userManager.CreateAsync(user, password);

            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear {email}: {Describe(created)}");
            }

            logger.LogInformation(
                "Usuario demo {Email} creado.", email);
        }

        // Cada cuenta demo conserva únicamente su rol asignado.
        IList<string> existingRoles =
            await userManager.GetRolesAsync(user);

        string[] unexpectedRoles = existingRoles
            .Where(existingRole =>
                !string.Equals(
                    existingRole, role,
                    StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (unexpectedRoles.Length > 0)
        {
            IdentityResult removed =
                await userManager.RemoveFromRolesAsync(
                    user, unexpectedRoles);

            if (!removed.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudieron corregir los roles de {email}: {Describe(removed)}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            IdentityResult assigned =
                await userManager.AddToRoleAsync(user, role);

            if (!assigned.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo asignar el rol {role} a {email}: {Describe(assigned)}");
            }

            logger.LogInformation(
                "Usuario {Email} asignado al rol {Role}.", email, role);
        }
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            throw new InvalidOperationException(
                $"No existe el rol {role}. Ejecuta primero PermissionsSeeder.");
        }
    }

    private static string GetConfiguredEmail(
        IConfiguration configuration,
        string key,
        string defaultEmail)
    {
        string? email = configuration[key];

        return string.IsNullOrWhiteSpace(email)
            ? defaultEmail
            : email.Trim();
    }

    private static string GetRequiredPassword(
        IConfiguration configuration,
        string key)
    {
        string? password = configuration[key];

        if (string.IsNullOrWhiteSpace(password))
        {
            string environmentVariable =
                key.Replace(":", "__", StringComparison.Ordinal);

            throw new InvalidOperationException(
                $"Falta {key}. Configúrala en appsettings.Development.json " +
                $"o mediante la variable de entorno {environmentVariable}.");
        }

        return password;
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
