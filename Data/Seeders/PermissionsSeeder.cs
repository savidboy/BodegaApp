using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data.Seeders;

// Corre en cada arranque: crea lo que falte y no toca lo que ya existe
public static class PermissionsSeeder
{
    // Lo que puede hacer el rol operativo (Admin no lo necesita, pero se le asigna igual)
    private static readonly string[] WarehousePermissions =
    [
        Permissions.HomeShow,
        Permissions.SuppliersShow,
        Permissions.SuppliersCreate,
        Permissions.SuppliersUpdate
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        BodegaDbContext context = services.GetRequiredService<BodegaDbContext>();
        RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(PermissionsSeeder));

        foreach (string role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Rol {Role} creado.", role);
            }
        }

        // Se inserta lo que falte; los permisos ya creados no se tocan
        foreach (PermissionSeed seed in Permissions.All)
        {
            bool exists = await context.Permissions.AnyAsync(permission => permission.Code == seed.Code);

            if (!exists)
            {
                context.Permissions.Add(new Permission
                {
                    Code = seed.Code,
                    Description = seed.Description,
                    Module = seed.Module
                });
            }
        }

        await context.SaveChangesAsync();

        IdentityRole adminRole = await roleManager.FindByNameAsync(Roles.Admin)
            ?? throw new InvalidOperationException($"No existe el rol {Roles.Admin}.");

        IdentityRole warehouseRole = await roleManager.FindByNameAsync(Roles.Warehouse)
            ?? throw new InvalidOperationException($"No existe el rol {Roles.Warehouse}.");

        await GrantAsync(context, adminRole.Id, Permissions.All.Select(seed => seed.Code));
        await GrantAsync(context, warehouseRole.Id, WarehousePermissions);
    }

    private static async Task GrantAsync(BodegaDbContext context, string roleId, IEnumerable<string> codes)
    {
        List<string> codeList = codes.ToList();

        List<int> permissionIds = await context.Permissions
            .Where(permission => codeList.Contains(permission.Code))
            .Select(permission => permission.Id)
            .ToListAsync();

        List<int> grantedIds = await context.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .Select(rolePermission => rolePermission.PermissionId)
            .ToListAsync();

        // Solo agrega: si alguien quitó un permiso desde la vista de roles, no vuelve
        foreach (int permissionId in permissionIds.Except(grantedIds))
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }

        await context.SaveChangesAsync();
    }
}
