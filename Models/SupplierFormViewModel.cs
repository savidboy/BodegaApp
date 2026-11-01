using System.ComponentModel.DataAnnotations;

namespace BodegaApp.Models;

// Campos del formulario de proveedor: un modelo propio para no aceptar columnas de más.
public class SupplierFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

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
