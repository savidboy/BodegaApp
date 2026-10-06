using System.ComponentModel.DataAnnotations;

namespace BodegaApp.Domain.Entities
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}