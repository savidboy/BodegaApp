
using Microsoft.EntityFrameworkCore;

namespace BodegaApp.Data.Seeders;

public static class SeedDb
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IHostEnvironment environment)
    {
        using IServiceScope scope = services.CreateScope();

        BodegaDbContext context =
            scope.ServiceProvider.GetRequiredService<BodegaDbContext>();

        // Las migraciones se ejecutan en todos los entornos.
        await context.Database.MigrateAsync();

        // Los roles y permisos son necesarios para el funcionamiento real.
        await PermissionsSeeder.SeedAsync(scope.ServiceProvider);

        // Los datos de demostración solo se crean en Development.
        if (environment.IsDevelopment())
        {
            await DemoUsersSeeder.SeedAsync(scope.ServiceProvider);
            await SuppliersSeeder.SeedAsync(context);
        }
    }
}
