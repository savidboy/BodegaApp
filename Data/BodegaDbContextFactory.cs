using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BodegaApp.Data;

/// <summary>
/// Factory de design-time que usa la CLI de EF Core ("dotnet ef") para poder generar
/// migraciones sin levantar el host web (que ejecutaría migraciones y seed al arrancar).
/// </summary>
public class BodegaDbContextFactory : IDesignTimeDbContextFactory<BodegaDbContext>
{
    public BodegaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=127.0.0.1;Port=5433;Database=bodega_db;Username=savid;Password=dev_password_bodega_2026";

        var options = new DbContextOptionsBuilder<BodegaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BodegaDbContext(options);
    }
}
