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

        // Sin cadena de conexión no se puede generar nada; nada de credenciales por defecto aquí
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Falta ConnectionStrings:DefaultConnection. Definila en appsettings.Development.json " +
                "o en la variable de entorno ConnectionStrings__DefaultConnection antes de usar 'dotnet ef'.");

        var options = new DbContextOptionsBuilder<BodegaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BodegaDbContext(options);
    }
}
