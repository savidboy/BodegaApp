using Microsoft.AspNetCore.Identity;

namespace BodegaApp.Domain.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
