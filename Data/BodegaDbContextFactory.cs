using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BodegaApp.Data;

// Lo usa "dotnet ef" para crear migraciones sin arrancar el host: el arranque
// aplica migraciones y siembra datos, y eso estorba desde la CLI.
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
            ?? throw new InvalidOperationException(
                "Falta ConnectionStrings:DefaultConnection. Definila en appsettings.Development.json " +
                "o en ConnectionStrings__DefaultConnection.");

        var options = new DbContextOptionsBuilder<BodegaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BodegaDbContext(options);
    }
}
