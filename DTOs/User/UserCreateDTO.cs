using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.User
{
    public class UserCreateDTO
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string Role { get; set; } = string.Empty;
    }
}