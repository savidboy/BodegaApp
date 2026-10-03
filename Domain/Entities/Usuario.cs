using Microsoft.AspNetCore.Identity;

namespace BodegaApp.Domain.Entities;

public class Usuario : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
}