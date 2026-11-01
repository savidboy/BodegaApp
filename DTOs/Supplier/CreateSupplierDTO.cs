using System.ComponentModel.DataAnnotations;
using BodegaApp.Domain;

namespace BodegaApp.DTOs.Supplier;

public class CreateSupplierDTO : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El contacto no puede superar los 100 caracteres.")]
    [Display(Name = "Contacto")]
    public string? Contact { get; set; }

    [StringLength(50, ErrorMessage = "El tipo no puede superar los 50 caracteres.")]
    [Display(Name = "Tipo")]
    public string? Type { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    [Display(Name = "Correo")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    [Display(Name = "Dirección")]
    public string? Address { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type == SupplierTypes.Other)
        {
            yield return new ValidationResult(
                "Seleccione un tipo de la lista o escriba uno nuevo.",
                new[] { nameof(Type) });
        }
    }
}
