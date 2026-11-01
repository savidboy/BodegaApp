using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data.Seeders;

public static class SuppliersSeeder
{
    public static async Task SeedAsync(BodegaDbContext context)
    {
        if (await context.Suppliers.AnyAsync())
        {
            return;
        }

        context.Suppliers.AddRange(
            new Supplier
            {
                Name = "Viñedo San Rafael",
                Contact = "Marta Ospina",
                Type = "Uva",
                Phone = "310 555 1201",
                Email = "compras@sanrafael.co",
                Address = "Vereda El Alto, Km 4"
            },
            new Supplier
            {
                Name = "Barricas del Valle",
                Contact = "Julián Restrepo",
                Type = "Barricas",
                Phone = "315 555 8834",
                Email = "ventas@barricasdelvalle.co",
                Address = "Carrera 15 # 32-10"
            },
            new Supplier
            {
                Name = "Corchos del Norte",
                Contact = "Ana Beltrán",
                Type = "Corchos",
                Phone = "320 555 4477",
                Email = "pedidos@corchosdelnorte.co",
                Address = "Calle 8 # 5-44"
            },
            new Supplier
            {
                Name = "Etiquetas Andinas",
                Contact = "Diego Salcedo",
                Type = "Etiquetas",
                Phone = "301 555 6620",
                Email = "contacto@etiquetasandinas.co",
                Address = "Zona Industrial, Bodega 12"
            });

        await context.SaveChangesAsync();
    }
}
