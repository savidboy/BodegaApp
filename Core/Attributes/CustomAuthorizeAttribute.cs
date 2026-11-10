using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Core.Attributes;

// Se comprueba contra la base en cada request. Ocultar el botón en la vista no
// alcanza: la URL se puede escribir a mano.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class CustomAuthorizeAttribute : TypeFilterAttribute
{
    public CustomAuthorizeAttribute(string permissionCode) : base(typeof(CustomAuthorizeFilter))
    {
        Arguments = [permissionCode];
    }
}

public class CustomAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly string _permissionCode;
    private readonly IAccountService _accountService;

    public CustomAuthorizeFilter(string permissionCode, IAccountService accountService)
    {
        _permissionCode = permissionCode;
        _accountService = accountService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!_accountService.AccountUserIsAuthenticated())
        {
            context.Result = new ChallengeResult();
            return;
        }

        bool authorized = await _accountService.AccountUserIsAuthorizedAsync(_permissionCode);

        if (!authorized)
        {
            // AccessDenied responde 403, así se ve el código real
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
