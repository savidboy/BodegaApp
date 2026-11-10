using System.ComponentModel.DataAnnotations;
using BodegaApp.Data.Abstractions;

namespace BodegaApp.Domain.Entities;

public class Permission : IId
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(128)]
    public string Description { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Module { get; set; } = null!;

    public List<RolePermission> RolePermissions { get; set; } = new();
}
