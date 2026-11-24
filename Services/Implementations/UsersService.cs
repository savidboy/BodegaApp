using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.Data;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;
using BodegaApp.DTOs.Role;
using BodegaApp.DTOs.User;
using BodegaApp.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BodegaApp.Services.Implementations;

public class UsersService : IUsersService
{
    private readonly UserManager<User> _userManager;
    private readonly BodegaDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UsersService> _logger;

    public UsersService(
        UserManager<User> userManager,
        BodegaDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UsersService> logger)
    {
        _userManager = userManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Response<PaginationResponse<UserDTO>>> GetPaginationAsync(UserPaginationRequest request)
    {
        try
        {
            IQueryable<User> query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(user => user.FullName.ToLower().Contains(filter)
                    || (user.Email != null && user.Email.ToLower().Contains(filter)));
            }

            int totalCount = await query.CountAsync();

            List<User> users = await query
                .OrderBy(user => user.FullName)
                .Skip((request.Page - 1) * request.RecordsPerPage)
                .Take(request.RecordsPerPage)
                .ToListAsync();

            List<string> userIds = users.Select(user => user.Id).ToList();

            // Los roles de toda la página se traen en dos consultas, no una por usuario
            List<IdentityUserRole<string>> userRoles = await _context.UserRoles
                .Where(userRole => userIds.Contains(userRole.UserId))
                .AsNoTracking()
                .ToListAsync();

            List<IdentityRole> roles = await _context.Roles.AsNoTracking().ToListAsync();

            Dictionary<string, IdentityRole> rolesById = roles.ToDictionary(role => role.Id);

            Dictionary<string, List<string>> roleNamesByUser = userRoles
                .Where(userRole => rolesById.ContainsKey(userRole.RoleId))
                .GroupBy(userRole => userRole.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(userRole => rolesById[userRole.RoleId].Name ?? string.Empty)
                        .OrderBy(name => name)
                        .ToList());

            Dictionary<string, List<string>> roleIdsByUser = userRoles
                .GroupBy(userRole => userRole.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(userRole => userRole.RoleId).ToList());

            List<UserDTO> items = users
                .Select(user => new UserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Roles = roleNamesByUser.GetValueOrDefault(user.Id) ?? new List<string>(),
                    RoleIds = roleIdsByUser.GetValueOrDefault(user.Id) ?? new List<string>()
                })
                .ToList();

            PaginationResponse<UserDTO> pagination = new()
            {
                CurrentPage = request.Page,
                RecordsPerPage = request.RecordsPerPage,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.RecordsPerPage),
                Filter = request.Filter,
                List = new PagedList<UserDTO>(items, totalCount, request.Page, request.RecordsPerPage),
                RouteValues = new Dictionary<string, string>
                {
                    ["filter"] = request.Filter?.Trim() ?? string.Empty,
                    ["recordsPerPage"] = request.RecordsPerPage.ToString()
                }
            };

            return Response<PaginationResponse<UserDTO>>.Success(pagination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo obtener la lista de usuarios.");

            return Response<PaginationResponse<UserDTO>>.Failure("No se pudo obtener la lista de usuarios.");
        }
    }

    public async Task<Response<UserDTO>> GetOneAsync(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return Response<UserDTO>.Failure("No existe el usuario.");
        }

        return Response<UserDTO>.Success(await BuildDtoAsync(user));
    }

    public async Task<Response<List<RoleOptionDTO>>> GetAllRolesAsync()
    {
        List<RoleOptionDTO> roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new RoleOptionDTO
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            })
            .ToListAsync();

        return Response<List<RoleOptionDTO>>.Success(roles);
    }

    public async Task<Response<UserDTO>> CreateAsync(CreateUserDTO dto)
    {
        string email = dto.Email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Response<UserDTO>.Failure("Ese correo ya está en uso.");
        }

        User user = new()
        {
            UserName = email,
            Email = email,
            FullName = dto.FullName.Trim(),
            EmailConfirmed = true
        };

        IdentityResult result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return Response<UserDTO>.Failure(
                "No se pudo crear el usuario.",
                result.Errors.Select(error => error.Description).ToArray());
        }

        await AssignRolesAsync(user, dto.SelectedRoleIds, replace: false);

        return Response<UserDTO>.Success(await BuildDtoAsync(user), "Usuario creado correctamente.");
    }

    public async Task<Response<UserDTO>> UpdateAsync(UpdateUserDTO dto)
    {
        User? user = await _userManager.FindByIdAsync(dto.Id);

        if (user is null)
        {
            return Response<UserDTO>.Failure("No existe el usuario.");
        }

        string email = dto.Email.Trim();

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            User? owner = await _userManager.FindByEmailAsync(email);

            if (owner is not null && owner.Id != user.Id)
            {
                return Response<UserDTO>.Failure("Ese correo ya lo usa otro usuario.");
            }

            user.Email = email;
            user.UserName = email;

            // El correo nuevo queda sin verificar
            user.EmailConfirmed = false;
        }

        user.FullName = dto.FullName.Trim();

        IdentityResult result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return Response<UserDTO>.Failure(
                "No se pudo actualizar el usuario.",
                result.Errors.Select(error => error.Description).ToArray());
        }

        await AssignRolesAsync(user, dto.SelectedRoleIds, replace: true);

        return Response<UserDTO>.Success(await BuildDtoAsync(user), "Usuario actualizado correctamente.");
    }

    public async Task<Response> DeleteAsync(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return Response.Failure("No existe el usuario.");
        }

        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        if (principal is not null && _userManager.GetUserId(principal) == user.Id)
        {
            return Response.Failure("No puedes eliminar el usuario con el que iniciaste sesión.");
        }

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            IList<User> admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

            if (admins.Count <= 1)
            {
                return Response.Failure("No se puede eliminar el último administrador.");
            }
        }

        IdentityResult result = await _userManager.DeleteAsync(user);

        return result.Succeeded
            ? Response.Success("Usuario eliminado correctamente.")
            : Response.Failure(
                "No se pudo eliminar el usuario.",
                result.Errors.Select(error => error.Description).ToArray());
    }

    private async Task<UserDTO> BuildDtoAsync(User user)
    {
        List<IdentityRole> roles = await (from userRole in _context.UserRoles
                                          join role in _context.Roles on userRole.RoleId equals role.Id
                                          where userRole.UserId == user.Id
                                          select role)
                                         .AsNoTracking()
                                         .ToListAsync();

        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.Select(role => role.Name ?? string.Empty).OrderBy(name => name).ToList(),
            RoleIds = roles.Select(role => role.Id).ToList()
        };
    }

    // Los roles del usuario se reemplazan por completo: la lista viene del formulario
    private async Task AssignRolesAsync(User user, List<string> selectedRoleIds, bool replace)
    {
        List<string> ids = selectedRoleIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        List<string> roleNames = await _context.Roles
            .Where(role => ids.Contains(role.Id))
            .Select(role => role.Name!)
            .ToListAsync();

        if (replace)
        {
            List<string> currentRoles = (await _userManager.GetRolesAsync(user)).ToList();

            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
        }

        if (roleNames.Count > 0)
        {
            await _userManager.AddToRolesAsync(user, roleNames);
        }
    }
}
