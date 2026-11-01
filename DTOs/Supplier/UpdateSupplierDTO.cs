using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.Supplier;

public class UpdateSupplierDTO : CreateSupplierDTO
{
    [Required]
    public int Id { get; set; }
}
