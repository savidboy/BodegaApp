using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.User
{
    public class UserEditDTO
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; } // Opcional al editar

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string Role { get; set; } = string.Empty;
    }
}