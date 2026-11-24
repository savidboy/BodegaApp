using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.DTOs.Role;

namespace BodegaApp.Services.Abstractions;

public interface IRolesService
{
    Task<Response<PaginationResponse<RoleDTO>>> GetPaginationAsync(RolePaginationRequest request);

    Task<Response<RoleDTO>> GetOneAsync(string id);

    Task<Response<List<PermissionDTO>>> GetPermissionsAsync();

    Task<Response<RoleDTO>> CreateAsync(RoleDTO dto);

    Task<Response<RoleDTO>> UpdateAsync(RoleDTO dto);

    Task<Response> DeleteAsync(string id);
}
