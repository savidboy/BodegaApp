using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.Role;

public class RoleDTO
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(256, ErrorMessage = "El nombre del rol no puede superar los 256 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public int PermissionCount { get; set; }

    public List<int> SelectedPermissionIds { get; set; } = new();

    public List<PermissionDTO> Permissions { get; set; } = new();
}
