using BodegaApp.Core;
using BodegaApp.Core.Attributes;
using BodegaApp.Core.Pagination;
using BodegaApp.Domain;
using BodegaApp.DTOs.Role;
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
    public async Task<IActionResult> Index([FromQuery] UserPaginationRequest request)
    {
        Response<PaginationResponse<UserDTO>> response = await _usersService.GetPaginationAsync(request);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return View(new PaginationResponse<UserDTO>());
        }

        return View(response.Result);
    }

    [HttpGet]
    [CustomAuthorize(Permissions.UsersCreate)]
    public async Task<IActionResult> Create()
    {
        CreateUserDTO model = new()
        {
            Roles = await GetRolesAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.UsersCreate)]
    public async Task<IActionResult> Create(CreateUserDTO model)
    {
        model.Roles = await GetRolesAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Response<UserDTO> response = await _usersService.CreateAsync(model);

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

        Response<UserDTO> response = await _usersService.GetOneAsync(id);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;

            return RedirectToAction(nameof(Index));
        }

        UpdateUserDTO model = new()
        {
            Id = response.Result.Id,
            FullName = response.Result.FullName,
            Email = response.Result.Email,
            SelectedRoleIds = response.Result.RoleIds,
            Roles = await GetRolesAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [CustomAuthorize(Permissions.UsersUpdate)]
    public async Task<IActionResult> Edit(UpdateUserDTO model)
    {
        model.Roles = await GetRolesAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Response<UserDTO> response = await _usersService.UpdateAsync(model);

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

        Response<UserDTO> response = await _usersService.GetOneAsync(id);

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
        Response response = await _usersService.DeleteAsync(id);

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

    private async Task<List<RoleOptionDTO>> GetRolesAsync()
    {
        Response<List<RoleOptionDTO>> response = await _usersService.GetAllRolesAsync();

        return response.Result ?? [];
    }
}
