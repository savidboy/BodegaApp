using System.Threading.Tasks;
using BodegaApp.Core.Attributes;
using BodegaApp.Domain;
using BodegaApp.DTOs.User;
using BodegaApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BodegaApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly UsersService _usersService;

        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [CustomAuthorize(Permissions.UsersShow)]
        public async Task<IActionResult> Index()
        {
            var users = await _usersService.GetAllUsersAsync();
            return View(users);
        }

        [CustomAuthorize(Permissions.UsersCreate)]
        public async Task<IActionResult> Create()
        {
            var roles = await _usersService.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.UsersCreate)]
        public async Task<IActionResult> Create(UserCreateDTO model)
        {
            if (ModelState.IsValid)
            {
                var success = await _usersService.CreateUserAsync(model);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "No se pudo crear el usuario. Verifique los datos.");
            }

            var roles = await _usersService.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles);
            return View(model);
        }

        [CustomAuthorize(Permissions.UsersUpdate)]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _usersService.GetUserForEditAsync(id);
            if (user == null) return NotFound();

            var roles = await _usersService.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.UsersUpdate)]
        public async Task<IActionResult> Edit(UserEditDTO model)
        {
            if (ModelState.IsValid)
            {
                var success = await _usersService.UpdateUserAsync(model);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el usuario.");
            }

            var roles = await _usersService.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.UsersDelete)]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _usersService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}