using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BodegaApp.Helpers
{
    public class AuthHelper : IAuthHelper
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthHelper(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task LogoutAndInvalidateSessionAsync(ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);

            if (user != null)
            {
                // Invalida sesiones previas (exigencia de la rúbrica)
                await _userManager.UpdateSecurityStampAsync(user);
            }

            // Cierra la sesión actual
            await _signInManager.SignOutAsync();
        }
    }
}