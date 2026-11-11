using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BodegaApp.Core;
using BodegaApp.Models;
using BodegaApp.Services.Abstractions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using BodegaApp.DTOs.Account;

namespace BodegaApp.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Response<SignInResult> response = await _accountService.LoginAsync(
            model.Email, model.Password, model.RememberMe);

        if (response.IsSuccess)
        {
            // IsLocalUrl evita que el login sirva para sacar al usuario del sitio
            return Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, response.Message);

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> UpdateUser()
    {
        var response = await _accountService.GetCurrentUserAsync();

        if (!response.IsSuccess || response.Result is null)
        {
            return RedirectToAction(nameof(Login));
        }

        AccountUserDTO dto = new AccountUserDTO
        {
            Id = response.Result.Id,
            FullName = response.Result.FullName,
            Email = response.Result.Email ?? string.Empty
        };

        return View(dto);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(AccountUserDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var response = await _accountService.UpdateUserAsync(dto);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(dto);
        }

        return RedirectToAction(nameof(UpdateUser));
}

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();

        return RedirectToAction(nameof(Login));
    }

    // Identity manda aquí al usuario que ya entró pero no tiene el permiso
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
