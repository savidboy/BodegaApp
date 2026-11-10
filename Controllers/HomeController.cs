using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BodegaApp.Core.Attributes;
using BodegaApp.Domain;
using BodegaApp.Models;

namespace BodegaApp.Controllers;

public class HomeController : Controller
{
    [CustomAuthorize(Permissions.HomeShow)]
    public IActionResult Index()
    {
        return View();
    }

    [CustomAuthorize(Permissions.HomeShow)]
    public IActionResult Privacy()
    {
        return View();
    }

    // Debe ser anónima: el usuario la ve aunque no haya iniciado sesión
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
