using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Core;
using BodegaApp.Data;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Services.Implementations;

public class AccountService : IAccountService
{
    private const string PermissionCacheKey = "bodega.permisos";

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly BodegaDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        BodegaDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response<SignInResult>> LoginAsync(string email, string password, bool rememberMe)
    {
        SignInResult result = await _signInManager.PasswordSignInAsync(
            email, password, rememberMe, lockoutOnFailure: true);

        return new Response<SignInResult>
        {
            IsSuccess = result.Succeeded,
            Result = result,
            Message = result.Succeeded
                ? "Sesión iniciada correctamente."
                : "Correo o contraseña incorrectos."
        };
    }

    public async Task LogoutAsync()
    {
        // Cambiar el security stamp invalida las cookies ya emitidas: cerrar sesión
        // aquí también cierra las demás sesiones abiertas del usuario
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        if (principal is not null)
        {
            User? user = await _userManager.GetUserAsync(principal);

            if (user is not null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }
        }

        await _signInManager.SignOutAsync();
    }

    public bool AccountUserIsAuthenticated()
    {
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        return principal?.Identity is not null && principal.Identity.IsAuthenticated;
    }

    public async Task<bool> AccountUserIsAuthorizedAsync(string code)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;
        ClaimsPrincipal? principal = httpContext?.User;

        if (httpContext is null || principal?.Identity is null || !principal.Identity.IsAuthenticated)
        {
            return false;
        }

        // Admin no pasa por la tabla
        if (principal.IsInRole(Roles.Admin))
        {
            return true;
        }

        HashSet<string> grantedCodes = await GetGrantedCodesAsync(httpContext, principal);

        return grantedCodes.Contains(code);
    }

    public Task<bool> AccountUserIsSuperAdminAsync()
    {
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        bool isSuperAdmin = principal?.Identity is not null
            && principal.Identity.IsAuthenticated
            && principal.IsInRole(Roles.Admin);

        return Task.FromResult(isSuperAdmin);
    }

    public async Task<Response<User>> GetCurrentUserAsync()
    {
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity is null || !principal.Identity.IsAuthenticated)
        {
            return Response<User>.Failure("No hay una sesión activa.");
        }

        User? user = await _userManager.GetUserAsync(principal);

        return user is null
            ? Response<User>.Failure("No se encontró el usuario de la sesión.")
            : Response<User>.Success(user, "Usuario de la sesión.");
    }

    private async Task<HashSet<string>> GetGrantedCodesAsync(HttpContext httpContext, ClaimsPrincipal principal)
    {
        // Se guarda por request: una vista que consulta 8 permisos no hace 8 consultas
        if (httpContext.Items[PermissionCacheKey] is HashSet<string> cached)
        {
            return cached;
        }

        string? userId = _userManager.GetUserId(principal);

        List<string> codes = userId is null
            ? []
            : await _context.RolePermissions
                .Where(rolePermission => _context.UserRoles
                    .Any(userRole => userRole.UserId == userId && userRole.RoleId == rolePermission.RoleId))
                .Select(rolePermission => rolePermission.Permission!.Code)
                .ToListAsync();

        HashSet<string> grantedCodes = codes.ToHashSet();

        httpContext.Items[PermissionCacheKey] = grantedCodes;

        return grantedCodes;
    }
}
