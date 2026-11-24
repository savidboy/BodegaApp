using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BodegaApp.Core;
using BodegaApp.Core.Attributes;
using BodegaApp.Domain;
using BodegaApp.DTOs.Home;
using BodegaApp.Models;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Controllers;

public class HomeController : Controller
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    [CustomAuthorize(Permissions.HomeShow)]
    public async Task<IActionResult> Index()
    {
        Response<HomeDashboardDTO> response = await _homeService.GetDashboardAsync();

        return View(response.Result ?? new HomeDashboardDTO());
    }

    // Debe ser anónima: el usuario la ve aunque no haya iniciado sesión
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
