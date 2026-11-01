using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BodegaApp.Domain.Entities;
using BodegaApp.Models;

namespace BodegaApp.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;

    public AccountController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Bloqueo temporal tras varios intentos fallidos
        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, true);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "Correo o contraseña incorrectos");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    // Identity manda aquí al usuario que ya entró pero no tiene el rol necesario
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}