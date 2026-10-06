using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data;

public class BodegaDbContext : IdentityDbContext<Usuario>
{
    public BodegaDbContext(DbContextOptions<BodegaDbContext> options) : base(options) { }
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
}