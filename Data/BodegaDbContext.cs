using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data;

public class BodegaDbContext : IdentityDbContext<User>
{
    public BodegaDbContext(DbContextOptions<BodegaDbContext> options) : base(options) { }
    public DbSet<Supplier> Suppliers => Set<Supplier>();
}
