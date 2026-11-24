using BodegaApp.Core;
using BodegaApp.Core.Attributes;
using BodegaApp.Core.Pagination;
using BodegaApp.Domain;
using BodegaApp.DTOs.Role;
using BodegaApp.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BodegaApp.Controllers;

public class RolesController : Controller
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpGet]
    [CustomAuthorize(Permissions.RolesShow)]
    public async Task<IActionResult> Index([FromQuery] RolePaginationRequest request)
    {
        Response<PaginationResponse<RoleDTO>> response = await _rolesService.GetPaginationAsync(request);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return View(new PaginationResponse<RoleDTO>());
        }

        return View(response.Result);
    }

    [HttpGet]
    [CustomAuthorize(Permissions.RolesCreate)]
    public async Task<IActionResult> Create()
    {
        RoleDTO model = new()
        {
            Permissions = await GetPermissionsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.RolesCreate)]
    public async Task<IActionResult> Create(RoleDTO model)
    {
        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync(model);
            return View(model);
        }

        Response<RoleDTO> response = await _rolesService.CreateAsync(model);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);

            foreach (string error in response.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await LoadPermissionsAsync(model);
            return View(model);
        }

        TempData["Success"] = response.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [CustomAuthorize(Permissions.RolesUpdate)]
    public async Task<IActionResult> Edit(string id)
    {
        Response<RoleDTO> response = await _rolesService.GetOneAsync(id);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(nameof(Index));
        }

        if (response.Result.Name == Roles.Admin)
        {
            TempData["Error"] = "El rol Admin no se edita: tiene acceso a todo por encima de la tabla de permisos.";

            return RedirectToAction(nameof(Index));
        }

        return View(response.Result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.RolesUpdate)]
    public async Task<IActionResult> Edit(string id, RoleDTO model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync(model);
            return View(model);
        }

        Response<RoleDTO> response = await _rolesService.UpdateAsync(model);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);

            foreach (string error in response.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            await LoadPermissionsAsync(model);
            return View(model);
        }

        TempData["Success"] = response.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [CustomAuthorize(Permissions.RolesDelete)]
    public async Task<IActionResult> Delete(string id)
    {
        Response<RoleDTO> response = await _rolesService.GetOneAsync(id);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(nameof(Index));
        }

        if (response.Result.Name == Roles.Admin)
        {
            TempData["Error"] = "El rol Admin no se puede eliminar.";

            return RedirectToAction(nameof(Index));
        }

        return View(response.Result);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.RolesDelete)]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        Response response = await _rolesService.DeleteAsync(id);

        if (response.IsSuccess)
        {
            TempData["Success"] = response.Message;
        }
        else
        {
            TempData["Error"] = response.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<PermissionDTO>> GetPermissionsAsync()
    {
        Response<List<PermissionDTO>> response = await _rolesService.GetPermissionsAsync();

        return response.Result ?? [];
    }

    private async Task LoadPermissionsAsync(RoleDTO model)
    {
        model.Permissions = await GetPermissionsAsync();
    }
}
