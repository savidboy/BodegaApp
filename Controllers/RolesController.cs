using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Core.Attributes;
using BodegaApp.Data;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;
using BodegaApp.Models;

namespace BodegaApp.Controllers;

public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly BodegaDbContext _context;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        BodegaDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    // LISTAR ROLES
    [HttpGet]
    [CustomAuthorize(Permissions.RolesShow)]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .ToListAsync();

        return View(roles);
    }

    // MOSTRAR FORMULARIO DE CREACIÓN
    [HttpGet]
    [CustomAuthorize(Permissions.RolesCreate)]
    public async Task<IActionResult> Create()
    {
        RoleFormViewModel model = new()
        {
            Permissions = await _context.Permissions
                .OrderBy(permission => permission.Module)
                .ThenBy(permission => permission.Description)
                .ToListAsync()
        };

        return View(model);
    }

    // CREAR ROL Y ASIGNAR PERMISOS
    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.RolesCreate)]
    public async Task<IActionResult> Create(RoleFormViewModel model)
    {
        model.RoleName = model.RoleName.Trim();

        if (await _roleManager.RoleExistsAsync(model.RoleName))
        {
            ModelState.AddModelError(
                nameof(model.RoleName),
                "Ya existe un rol con ese nombre.");
        }

        List<int> selectedIds = model.SelectedPermissionIds
            .Distinct()
            .ToList();

        List<Permission> permissions = await _context.Permissions
            .Where(permission => selectedIds.Contains(permission.Id))
            .ToListAsync();

        if (permissions.Count != selectedIds.Count)
        {
            ModelState.AddModelError(
                string.Empty,
                "Uno o más permisos seleccionados no son válidos.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPermissions(model);
            return View(model);
        }

        IdentityRole role = new(model.RoleName);

        IdentityResult result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadPermissions(model);
            return View(model);
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

        TempData["Success"] = "Rol creado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // MOSTRAR FORMULARIO DE EDICIÓN
    [HttpGet]
    [CustomAuthorize(Permissions.RolesUpdate)]
    public async Task<IActionResult> Edit(string id)
    {
        IdentityRole? role = await _roleManager.FindByIdAsync(id);

        if (role is null)
        {
            return NotFound();
        }

        List<int> selectedIds = await _context.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == id)
            .Select(rolePermission => rolePermission.PermissionId)
            .ToListAsync();

        RoleFormViewModel model = new()
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            SelectedPermissionIds = selectedIds,
            Permissions = await _context.Permissions
                .OrderBy(permission => permission.Module)
                .ThenBy(permission => permission.Description)
                .ToListAsync()
        };

        return View(model);
    }

    // ACTUALIZAR PERMISOS DEL ROL
    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.RolesUpdate)]
    public async Task<IActionResult> Edit(string id, RoleFormViewModel model)
    {
        if (id != model.RoleId)
        {
            return NotFound();
        }

        IdentityRole? role = await _roleManager.FindByIdAsync(id);

        if (role is null)
        {
            return NotFound();
        }

        List<int> selectedIds = model.SelectedPermissionIds
            .Distinct()
            .ToList();

        List<Permission> permissions = await _context.Permissions
            .Where(permission => selectedIds.Contains(permission.Id))
            .ToListAsync();

        if (permissions.Count != selectedIds.Count)
        {
            ModelState.AddModelError(
                string.Empty,
                "Uno o más permisos seleccionados no son válidos.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPermissions(model);
            return View(model);
        }

        List<RolePermission> currentPermissions = await _context.RolePermissions
            .Where(rolePermission => rolePermission.RoleId == id)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(currentPermissions);

        foreach (Permission permission in permissions)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = id,
                PermissionId = permission.Id
            });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Permisos del rol actualizados correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // CARGAR PERMISOS CUANDO HAY ERRORES DE VALIDACIÓN
    private async Task LoadPermissions(RoleFormViewModel model)
    {
        model.Permissions = await _context.Permissions
            .OrderBy(permission => permission.Module)
            .ThenBy(permission => permission.Description)
            .ToListAsync();
    }
}