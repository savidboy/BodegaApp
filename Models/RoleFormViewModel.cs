using System.ComponentModel.DataAnnotations;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Models;

public class RoleFormViewModel
{
    public string? RoleId { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [MaxLength(256, ErrorMessage = "El nombre del rol no puede superar los 256 caracteres.")]
    public string RoleName { get; set; } = string.Empty;

    public List<int> SelectedPermissionIds { get; set; } = new();

    public List<Permission> Permissions { get; set; } = new();
}
