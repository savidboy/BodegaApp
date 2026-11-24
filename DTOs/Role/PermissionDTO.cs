namespace BodegaApp.DTOs.Role;

public class PermissionDTO
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;
}
