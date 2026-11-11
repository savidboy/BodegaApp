using BodegaApp.Core;
using BodegaApp.DTOs.User;

namespace BodegaApp.Services.Abstractions;

public interface IUsersService
{
    Task<Response<List<UserViewModel>>> GetAllUsersAsync();

    Task<Response<UserViewModel>> GetOneAsync(string id);

    Task<Response<List<UserRoleOption>>> GetAllRolesAsync();

    Task<Response<bool>> CreateUserAsync(UserCreateDTO dto);

    Task<Response<bool>> UpdateUserAsync(UserEditDTO dto);

    Task<Response<bool>> DeleteUserAsync(string id);
}
