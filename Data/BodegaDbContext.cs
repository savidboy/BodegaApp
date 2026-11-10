using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Data;

public class BodegaDbContext : IdentityDbContext<User>
{
    public BodegaDbContext(DbContextOptions<BodegaDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // El código es el contrato con los controladores
        builder.Entity<Permission>()
            .HasIndex(permission => permission.Code)
            .IsUnique();

        builder.Entity<RolePermission>(entity =>
        {
            // Clave compuesta: la base rechaza el permiso repetido en el mismo rol
            entity.HasKey(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId });

            // Al borrar el rol o el permiso, sus concesiones se van con él
            entity.HasOne(rolePermission => rolePermission.Role)
                  .WithMany()
                  .HasForeignKey(rolePermission => rolePermission.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rolePermission => rolePermission.Permission)
                  .WithMany(permission => permission.RolePermissions)
                  .HasForeignKey(rolePermission => rolePermission.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
