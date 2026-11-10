using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BodegaApp.Core;
using BodegaApp.Core.Attributes;
using BodegaApp.Core.Pagination;
using BodegaApp.Domain;
using BodegaApp.DTOs.Supplier;
using BodegaApp.Services.Abstractions;

namespace BodegaApp.Controllers;

public class SuppliersController : Controller
{
    private readonly ISuppliersService _suppliersService;

    public SuppliersController(ISuppliersService suppliersService)
    {
        _suppliersService = suppliersService;
    }

    // Un proveedor inactivo es un proveedor "borrado": solo el admin lo sigue viendo
    private bool IncludeInactive => User.IsInRole(Roles.Admin);

    [HttpGet]
    [CustomAuthorize(Permissions.SuppliersShow)]
    public async Task<IActionResult> Index([FromQuery] SupplierPaginationRequest request)
    {
        request.IncludeInactive = request.IncludeInactive && IncludeInactive;

        Response<PaginationResponse<SupplierDTO>> response = await _suppliersService.GetPaginationAsync(request);

        if (!response.IsSuccess || response.Result is null)
        {
            TempData["Error"] = response.Message;
            return View(new PaginationResponse<SupplierDTO>());
        }

        return View(response.Result);
    }

    [HttpGet]
    [CustomAuthorize(Permissions.SuppliersShow)]
    public async Task<IActionResult> Details(int id)
    {
        Response<SupplierDTO> response = await _suppliersService.GetOneAsync(id, IncludeInactive);

        if (!response.IsSuccess)
        {
            return NotFound();
        }

        return View(response.Result);
    }

    [HttpGet]
    [CustomAuthorize(Permissions.SuppliersCreate)]
    public IActionResult Create()
    {
        return View(new CreateSupplierDTO());
    }

    [HttpPost]
    [CustomAuthorize(Permissions.SuppliersCreate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSupplierDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        Response<CreateSupplierDTO> response = await _suppliersService.CreateAsync(dto);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(dto);
        }

        TempData["Success"] = response.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [CustomAuthorize(Permissions.SuppliersUpdate)]
    public async Task<IActionResult> Edit(int id)
    {
        Response<SupplierDTO> response = await _suppliersService.GetOneAsync(id, IncludeInactive);

        if (!response.IsSuccess)
        {
            return NotFound();
        }

        SupplierDTO supplier = response.Result!;

        UpdateSupplierDTO dto = new()
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Contact = supplier.Contact,
            Type = supplier.Type,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive
        };

        return View(dto);
    }

    [HttpPost]
    [CustomAuthorize(Permissions.SuppliersUpdate)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSupplierDTO dto)
    {
        if (id != dto.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        Response<SupplierDTO> response = await _suppliersService.UpdateAsync(dto, IncludeInactive);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, response.Message);
            return View(dto);
        }

        TempData["Success"] = response.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [CustomAuthorize(Permissions.SuppliersDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        Response<SupplierDTO> response = await _suppliersService.GetOneAsync(id, IncludeInactive);

        if (!response.IsSuccess)
        {
            return NotFound();
        }

        return View(response.Result);
    }

    [HttpPost, ActionName("Delete")]
    [CustomAuthorize(Permissions.SuppliersDelete)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        Response response = await _suppliersService.DeactivateAsync(id, IncludeInactive);

        TempData[response.IsSuccess ? "Success" : "Error"] = response.Message;

        return RedirectToAction(nameof(Index));
    }

    // Borrado definitivo: sigue siendo solo para Admin, además del permiso
    [Authorize(Roles = Roles.Admin)]
    [CustomAuthorize(Permissions.SuppliersDelete)]
    [HttpGet]
    public async Task<IActionResult> HardDelete(int id)
    {
        Response<SupplierDTO> response = await _suppliersService.GetOneAsync(id, includeInactive: true);

        if (!response.IsSuccess)
        {
            return NotFound();
        }

        return View(response.Result);
    }

    [Authorize(Roles = Roles.Admin)]
    [CustomAuthorize(Permissions.SuppliersDelete)]
    [HttpPost, ActionName("HardDelete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDeleteConfirmed(int id)
    {
        Response response = await _suppliersService.DeleteAsync(id);

        TempData[response.IsSuccess ? "Success" : "Error"] = response.Message;

        return RedirectToAction(nameof(Index));
    }
}
