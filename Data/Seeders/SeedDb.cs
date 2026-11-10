using Microsoft.EntityFrameworkCore;

namespace BodegaApp.Data.Seeders;

public static class SeedDb
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        BodegaDbContext context = scope.ServiceProvider.GetRequiredService<BodegaDbContext>();

        await context.Database.MigrateAsync();

        // Primero roles y permisos: AccountSeeder necesita el rol Almacen para el usuario demo
        await PermissionsSeeder.SeedAsync(scope.ServiceProvider);
        await AccountSeeder.SeedAsync(scope.ServiceProvider);
        await SuppliersSeeder.SeedAsync(context);
    }
}
