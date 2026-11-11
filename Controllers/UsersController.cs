using BodegaApp.Core;
using BodegaApp.Core.Attributes;
using BodegaApp.Domain;
using BodegaApp.DTOs.User;
using BodegaApp.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BodegaApp.Controllers;

public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    [CustomAuthorize(Permissions.UsersShow)]
    public async Task<IActionResult> Index()
    {
        Response<List<UserViewModel>> response = await _usersService.GetAllUsersAsync();

        if (!response.IsSuccess)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(nameof(Index));
        }

        return View(response.Result);
    }

    [HttpGet]
    [CustomAuthorize(Permissions.UsersCreate)]
    public async Task<IActionResult> Create()
    {
        UserCreateDTO model = new()
        {
            Roles = await GetRolesAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.UsersCreate)]
    public async Task<IActionResult> Create(UserCreateDTO model)
    {
        model.Roles = await GetRolesAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Response<bool> response = await _usersService.CreateUserAsync(model);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);

            foreach (string error in response.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        TempData["Success"] = response.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [CustomAuthorize(Permissions.UsersUpdate)]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        Response<UserViewModel> response = await _usersService.GetOneAsync(id);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(nameof(Index));
        }

        UserEditDTO model = new()
        {
            Id = response.Result.Id,
            FullName = response.Result.FullName,
            Email = response.Result.Email,
            Roles = await GetRolesAsync()
        };

        List<UserRoleOption> roles = model.Roles;

        model.SelectedRoleIds = await GetSelectedRoleIdsAsync(response.Result.Id, roles);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.UsersUpdate)]
    public async Task<IActionResult> Edit(UserEditDTO model)
    {
        model.Roles = await GetRolesAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Response<bool> response = await _usersService.UpdateUserAsync(model);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);

            foreach (string error in response.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        TempData["Success"] = response.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [CustomAuthorize(Permissions.UsersDelete)]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        Response<UserViewModel> response = await _usersService.GetOneAsync(id);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(nameof(Index));
        }

        return View(response.Result);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.UsersDelete)]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        Response<bool> response = await _usersService.DeleteUserAsync(id);

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

    private async Task<List<UserRoleOption>> GetRolesAsync()
    {
        Response<List<UserRoleOption>> response = await _usersService.GetAllRolesAsync();

        return response.Result ?? [];
    }

    // Los roles que ya tiene el usuario salen marcados en el formulario
    private async Task<List<string>> GetSelectedRoleIdsAsync(string userId, List<UserRoleOption> roles)
    {
        Response<UserViewModel> response = await _usersService.GetOneAsync(userId);

        if (!response.IsSuccess || response.Result is null)
        {
            return [];
        }

        return roles
            .Where(role => response.Result.Roles.Contains(role.Name))
            .Select(role => role.Id)
            .ToList();
    }
}
