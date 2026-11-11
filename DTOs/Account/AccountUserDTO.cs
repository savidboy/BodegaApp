using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.Account;

public class AccountUserDTO
{
    [Required(ErrorMessage = "El identificador es obligatorio.")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    public string Email { get; set; } = string.Empty;
}