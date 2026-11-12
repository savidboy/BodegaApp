using System.Security.Claims;
using BodegaApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BodegaApp.Helpers;

public class AuthHelper : IAuthHelper
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthHelper(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task LogoutAndInvalidateSessionAsync(ClaimsPrincipal userPrincipal)
    {
        User? user = await _userManager.GetUserAsync(userPrincipal);

        if (user is not null)
        {
            // Invalida las cookies ya emitidas: cerrar sesión en un equipo
            // cierra también las sesiones abiertas en los demás
            await _userManager.UpdateSecurityStampAsync(user);
        }

        await _signInManager.SignOutAsync();
    }
}
