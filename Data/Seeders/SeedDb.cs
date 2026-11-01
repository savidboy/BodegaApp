using Microsoft.EntityFrameworkCore;

namespace BodegaApp.Data.Seeders;

public static class SeedDb
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        BodegaDbContext context = scope.ServiceProvider.GetRequiredService<BodegaDbContext>();

        await context.Database.MigrateAsync();

        await AccountSeeder.SeedAsync(scope.ServiceProvider);
        await SuppliersSeeder.SeedAsync(context);
    }
}
