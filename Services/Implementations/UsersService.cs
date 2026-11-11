using BodegaApp.Core;
using BodegaApp.Data;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;
using BodegaApp.DTOs.User;
using BodegaApp.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BodegaApp.Services.Implementations;

public class UsersService : IUsersService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly BodegaDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        BodegaDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<List<UserViewModel>>> GetAllUsersAsync()
    {
        List<User> users = await _userManager.Users.ToListAsync();

        // Los roles se traen en una sola consulta: si se pide rol por usuario
        // sale una consulta por cada usuario de la lista
        List<IdentityRole> roles = await _roleManager.Roles.ToListAsync();

        Dictionary<string, string> roleNames = roles.ToDictionary(role => role.Id, role => role.Name ?? string.Empty);

        List<IdentityUserRole<string>> userRoles = await _context.UserRoles.ToListAsync();

        Dictionary<string, List<string>> rolesByUser = userRoles
            .GroupBy(userRole => userRole.UserId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(userRole => roleNames.TryGetValue(userRole.RoleId, out string? name) ? name : userRole.RoleId)
                    .OrderBy(name => name)
                    .ToList());

        List<UserViewModel> result = users
            .Select(user => new UserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = rolesByUser.TryGetValue(user.Id, out List<string>? names) ? names : []
            })
            .OrderBy(user => user.FullName)
            .ToList();

        return Response<List<UserViewModel>>.Success(result);
    }

    public async Task<Response<UserViewModel>> GetOneAsync(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return Response<UserViewModel>.Failure("No existe el usuario.");
        }

        List<string> roles = (await _userManager.GetRolesAsync(user)).ToList();

        return Response<UserViewModel>.Success(new UserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.OrderBy(role => role).ToList()
        });
    }

    public async Task<Response<List<UserRoleOption>>> GetAllRolesAsync()
    {
        List<UserRoleOption> roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new UserRoleOption
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty
            })
            .ToListAsync();

        return Response<List<UserRoleOption>>.Success(roles);
    }

    public async Task<Response<bool>> CreateUserAsync(UserCreateDTO dto)
    {
        string email = dto.Email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Response<bool>.Failure("Ese correo ya está en uso.");
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
            return Response<bool>.Failure(
                "No se pudo crear el usuario.",
                result.Errors.Select(error => error.Description).ToArray());
        }

        await AssignRolesAsync(user, dto.SelectedRoleIds, replace: false);

        return Response<bool>.Success(true, "Usuario creado correctamente.");
    }

    public async Task<Response<bool>> UpdateUserAsync(UserEditDTO dto)
    {
        User? user = await _userManager.FindByIdAsync(dto.Id);

        if (user is null)
        {
            return Response<bool>.Failure("No existe el usuario.");
        }

        string email = dto.Email.Trim();

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            User? owner = await _userManager.FindByEmailAsync(email);

            if (owner is not null && owner.Id != user.Id)
            {
                return Response<bool>.Failure("Ese correo ya lo usa otro usuario.");
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
            return Response<bool>.Failure(
                "No se pudo actualizar el usuario.",
                result.Errors.Select(error => error.Description).ToArray());
        }

        if (!string.IsNullOrEmpty(dto.Password))
        {
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);

            if (!passwordResult.Succeeded)
            {
                return Response<bool>.Failure(
                    "No se pudo cambiar la contraseña.",
                    passwordResult.Errors.Select(error => error.Description).ToArray());
            }
        }

        await AssignRolesAsync(user, dto.SelectedRoleIds, replace: true);

        return Response<bool>.Success(true, "Usuario actualizado correctamente.");
    }

    public async Task<Response<bool>> DeleteUserAsync(string id)
    {
        User? user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return Response<bool>.Failure("No existe el usuario.");
        }

        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        if (principal is not null && _userManager.GetUserId(principal) == user.Id)
        {
            return Response<bool>.Failure("No puedes eliminar el usuario con el que iniciaste sesión.");
        }

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            IList<User> admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

            if (admins.Count <= 1)
            {
                return Response<bool>.Failure("No se puede eliminar el último administrador.");
            }
        }

        IdentityResult result = await _userManager.DeleteAsync(user);

        return result.Succeeded
            ? Response<bool>.Success(true, "Usuario eliminado correctamente.")
            : Response<bool>.Failure(
                "No se pudo eliminar el usuario.",
                result.Errors.Select(error => error.Description).ToArray());
    }

    // Los roles del usuario se reemplazan por completo: la lista viene del formulario
    private async Task AssignRolesAsync(User user, List<string> selectedRoleIds, bool replace)
    {
        List<string> ids = selectedRoleIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        List<string> roleNames = new();

        foreach (string roleId in ids)
        {
            IdentityRole? role = await _roleManager.FindByIdAsync(roleId);

            if (role is not null)
            {
                roleNames.Add(role.Name ?? string.Empty);
            }
        }

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
