using Microsoft.AspNetCore.Identity;
using BodegaApp.Core;
using BodegaApp.Domain.Entities;
using BodegaApp.DTOs.Account;

namespace BodegaApp.Services.Abstractions;

public interface IAccountService
{
    Task<Response<SignInResult>> LoginAsync(string email, string password, bool rememberMe);
    Task LogoutAsync();

    bool AccountUserIsAuthenticated();
    Task<bool> AccountUserIsAuthorizedAsync(string code);
    Task<bool> AccountUserIsSuperAdminAsync();

    Task<Response<User>> GetCurrentUserAsync();

    Task<Response<bool>> UpdateUserAsync(AccountUserDTO dto);
}
