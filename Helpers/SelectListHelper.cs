using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BodegaApp.Helpers
{
    public class SelectListHelper : ISelectListHelper
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public SelectListHelper(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<SelectListItem>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            return roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            });
        }
    }
}