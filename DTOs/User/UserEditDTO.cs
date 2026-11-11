using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.User;

public class UserEditDTO
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [StringLength(256, ErrorMessage = "El correo no puede superar los 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    // Vacía significa que la contraseña no cambia
    [StringLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
    public string? Password { get; set; }

    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un rol.")]
    public List<string> SelectedRoleIds { get; set; } = new();

    public List<UserRoleOption> Roles { get; set; } = new();
}
