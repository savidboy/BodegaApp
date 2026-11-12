using Microsoft.AspNetCore.Mvc.Rendering;

namespace BodegaApp.Helpers
{
    public interface ISelectListHelper
    {
        Task<IEnumerable<SelectListItem>> GetRolesAsync();
    }
} 