using System.Collections.Generic;
using System.Threading.Tasks;
using BodegaApp.DTOs.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BodegaApp.Services
{
    public class UsersService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = roles.Count > 0 ? roles[0] : "Sin Rol"
                });
            }

            return userList;
        }

        public async Task<bool> CreateUserAsync(UserCreateDTO model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                return true;
            }
            return false;
        }

        public async Task<UserEditDTO?> GetUserForEditAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserEditDTO
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Role = roles.Count > 0 ? roles[0] : string.Empty
            };
        }

        public async Task<bool> UpdateUserAsync(UserEditDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return false;

            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // Actualizar contraseña si se proporciona una nueva
            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, model.Password);
            }

            // Actualizar rol
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return true;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return roles;
        }
    }
}