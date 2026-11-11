using System.ComponentModel.DataAnnotations;

namespace BodegaApp.DTOs.Account;

public class AccountUserDTO
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;
}