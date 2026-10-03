using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BodegaApp.Domain.Entities;
using BodegaApp.Models;

namespace BodegaApp.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;

    public AccountController(SignInManager<Usuario> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, false);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "Correo o contraseña incorrectos");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}