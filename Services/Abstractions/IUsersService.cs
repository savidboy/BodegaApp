using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.DTOs.Role;
using BodegaApp.DTOs.User;

namespace BodegaApp.Services.Abstractions;

public interface IUsersService
{
    Task<Response<PaginationResponse<UserDTO>>> GetPaginationAsync(UserPaginationRequest request);

    Task<Response<UserDTO>> GetOneAsync(string id);

    Task<Response<List<RoleOptionDTO>>> GetAllRolesAsync();

    Task<Response<UserDTO>> CreateAsync(CreateUserDTO dto);

    Task<Response<UserDTO>> UpdateAsync(UpdateUserDTO dto);

    Task<Response> DeleteAsync(string id);
}
