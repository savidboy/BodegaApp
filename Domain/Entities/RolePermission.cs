using Microsoft.AspNetCore.Identity;

namespace BodegaApp.Domain.Entities;

/// <summary>
/// Tabla puente entre un rol de Identity y un permiso.
/// </summary>
public class RolePermission
{
    public string RoleId { get; set; } = null!;

    public int PermissionId { get; set; }

    public IdentityRole? Role { get; set; }

    public Permission? Permission { get; set; }
}
