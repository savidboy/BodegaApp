using System.Security.Claims;

namespace BodegaApp.Helpers;

public interface IAuthHelper
{
    Task LogoutAndInvalidateSessionAsync(ClaimsPrincipal userPrincipal);
}
