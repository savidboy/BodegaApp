using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Core;
using BodegaApp.Core.Pagination;
using BodegaApp.Data;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;
using BodegaApp.DTOs.Role;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Services.Implementations;

public class RolesService : IRolesService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly BodegaDbContext _context;
    private readonly ILogger<RolesService> _logger;

    public RolesService(
        RoleManager<IdentityRole> roleManager,
        BodegaDbContext context,
        ILogger<RolesService> logger)
    {
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task<Response<PaginationResponse<RoleDTO>>> GetPaginationAsync(RolePaginationRequest request)
    {
        try
        {
            IQueryable<IdentityRole> query = _roleManager.Roles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(role => role.Name!.ToLower().Contains(filter));
            }

            int totalCount = await query.CountAsync();

            List<IdentityRole> roles = await query
                .OrderBy(role => role.Name)
                .Skip((request.Page - 1) * request.RecordsPerPage)
                .Take(request.RecordsPerPage)
                .ToListAsync();

            List<string> roleIds = roles.Select(role => role.Id).ToList();

            Dictionary<string, int> permissionCounts = await _context.RolePermissions
                .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
                .GroupBy(rolePermission => rolePermission.RoleId)
                .Select(group => new { RoleId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.RoleId, row => row.Count);

            List<RoleDTO> items = roles
                .Select(role => new RoleDTO
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    PermissionCount = permissionCounts.GetValueOrDefault(role.Id)
                })
                .ToList();

            PaginationResponse<RoleDTO> pagination = new()
            {
                CurrentPage = request.Page,
                RecordsPerPage = request.RecordsPerPage,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.RecordsPerPage),
                Filter = request.Filter,
                List = new PagedList<RoleDTO>(items, totalCount, request.Page, request.RecordsPerPage),
                RouteValues = new Dictionary<string, string>
                {
                    ["filter"] = request.Filter?.Trim() ?? string.Empty,
                    ["recordsPerPage"] = request.RecordsPerPage.ToString()
                }
            };

            return Response<PaginationResponse<RoleDTO>>.Success(pagination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo obtener la lista de roles.");

            return Response<PaginationResponse<RoleDTO>>.Failure("No se pudo obtener la lista de roles.");
        }
    }

    public async Task<Response<RoleDTO>> GetOneAsync(string id)
    {
        IdentityRole? role = await _roleManager.FindByIdAsync(id);

        if (role is null)
        {
            return Response<RoleDTO>.Failure("No existe el rol.");
        }

        List<int> selectedPermissionIds = await _context.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == id)
            .Select(rolePermission => rolePermission.PermissionId)
            .ToListAsync();

        return Response<RoleDTO>.Success(new RoleDTO
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            PermissionCount = selectedPermissionIds.Count,
            SelectedPermissionIds = selectedPermissionIds,
            Permissions = await GetPermissionListAsync()
        });
    }

    public async Task<Response<List<PermissionDTO>>> GetPermissionsAsync()
    {
        return Response<List<PermissionDTO>>.Success(await GetPermissionListAsync());
    }

    public async Task<Response<RoleDTO>> CreateAsync(RoleDTO dto)
    {
        string name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return Response<RoleDTO>.Failure("El nombre del rol es obligatorio.");
        }

        if (await _roleManager.RoleExistsAsync(name))
        {
            return Response<RoleDTO>.Failure("Ya existe un rol con ese nombre.");
        }

        List<int> selectedIds = dto.SelectedPermissionIds.Distinct().ToList();
        List<Permission> permissions = await GetPermissionsByIdsAsync(selectedIds);

        if (permissions.Count != selectedIds.Count)
        {
            return Response<RoleDTO>.Failure("Uno o más permisos seleccionados no son válidos.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            IdentityRole role = new(name);

            IdentityResult result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();

                return Response<RoleDTO>.Failure(
                    "No se pudo crear el rol.",
                    result.Errors.Select(error => error.Description).ToArray());
            }

            foreach (Permission permission in permissions)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Response<RoleDTO>.Success(
                new RoleDTO
                {
                    Id = role.Id,
                    Name = name,
                    PermissionCount = permissions.Count,
                    SelectedPermissionIds = selectedIds
                },
                "Rol creado correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "No se pudo crear el rol {Name}.", name);

            return Response<RoleDTO>.Failure("No se pudo crear el rol.");
        }
    }

    public async Task<Response<RoleDTO>> UpdateAsync(RoleDTO dto)
    {
        IdentityRole? role = await _roleManager.FindByIdAsync(dto.Id);

        if (role is null)
        {
            return Response<RoleDTO>.Failure("No existe el rol.");
        }

        if (role.Name == Roles.Admin)
        {
            return Response<RoleDTO>.Failure(
                "El rol Admin no se edita: tiene acceso a todo por encima de la tabla de permisos.");
        }

        string name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return Response<RoleDTO>.Failure("El nombre del rol es obligatorio.");
        }

        if (!string.Equals(role.Name, name, StringComparison.Ordinal))
        {
            if (await _roleManager.RoleExistsAsync(name))
            {
                return Response<RoleDTO>.Failure("Ya existe un rol con ese nombre.");
            }
        }

        List<int> selectedIds = dto.SelectedPermissionIds.Distinct().ToList();
        List<Permission> permissions = await GetPermissionsByIdsAsync(selectedIds);

        if (permissions.Count != selectedIds.Count)
        {
            return Response<RoleDTO>.Failure("Uno o más permisos seleccionados no son válidos.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (!string.Equals(role.Name, name, StringComparison.Ordinal))
            {
                role.Name = name;

                IdentityResult renameResult = await _roleManager.UpdateAsync(role);

                if (!renameResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    return Response<RoleDTO>.Failure(
                        "No se pudo actualizar el rol.",
                        renameResult.Errors.Select(error => error.Description).ToArray());
                }
            }

            List<RolePermission> currentPermissions = await _context.RolePermissions
                .Where(rolePermission => rolePermission.RoleId == role.Id)
                .ToListAsync();

            List<int> currentIds = currentPermissions
                .Select(rolePermission => rolePermission.PermissionId)
                .ToList();

            // Se quitan solo los desmarcados y se agregan solo los nuevos:
            // borrar y reinsertar la misma clave en un solo SaveChanges rompe el tracking de EF
            _context.RolePermissions.RemoveRange(
                currentPermissions.Where(rolePermission => !selectedIds.Contains(rolePermission.PermissionId)));

            foreach (int permissionId in selectedIds.Except(currentIds))
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Response<RoleDTO>.Success(
                new RoleDTO
                {
                    Id = role.Id,
                    Name = name,
                    PermissionCount = permissions.Count,
                    SelectedPermissionIds = selectedIds
                },
                "Rol actualizado correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "No se pudo actualizar el rol {Id}.", dto.Id);

            return Response<RoleDTO>.Failure("No se pudo actualizar el rol.");
        }
    }

    public async Task<Response> DeleteAsync(string id)
    {
        IdentityRole? role = await _roleManager.FindByIdAsync(id);

        if (role is null)
        {
            return Response.Failure("No existe el rol.");
        }

        if (role.Name == Roles.Admin)
        {
            return Response.Failure("El rol Admin no se puede eliminar.");
        }

        int usersWithRole = await _context.UserRoles.CountAsync(userRole => userRole.RoleId == id);

        if (usersWithRole > 0)
        {
            return Response.Failure(
                $"No se puede eliminar el rol: {usersWithRole} usuario(s) lo tienen asignado.");
        }

        IdentityResult result = await _roleManager.DeleteAsync(role);

        return result.Succeeded
            ? Response.Success("Rol eliminado correctamente.")
            : Response.Failure(
                "No se pudo eliminar el rol.",
                result.Errors.Select(error => error.Description).ToArray());
    }

    private async Task<List<PermissionDTO>> GetPermissionListAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Module)
            .ThenBy(permission => permission.Description)
            .Select(permission => new PermissionDTO
            {
                Id = permission.Id,
                Code = permission.Code,
                Description = permission.Description,
                Module = permission.Module
            })
            .ToListAsync();
    }

    private async Task<List<Permission>> GetPermissionsByIdsAsync(List<int> selectedIds)
    {
        return await _context.Permissions
            .Where(permission => selectedIds.Contains(permission.Id))
            .ToListAsync();
    }
}
