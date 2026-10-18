using System.ComponentModel.DataAnnotations;

namespace BodegaApp.Domain.Entities
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(100)]
        public string? Contact { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
