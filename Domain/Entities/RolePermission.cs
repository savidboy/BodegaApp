using Microsoft.AspNetCore.Identity;

namespace BodegaApp.Domain.Entities;

public class RolePermission
{
    public string RoleId { get; set; } = null!;

    public int PermissionId { get; set; }

    public IdentityRole? Role { get; set; }

    public Permission? Permission { get; set; }
}
