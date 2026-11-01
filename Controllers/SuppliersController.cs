using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Data;
using BodegaApp.Domain;
using BodegaApp.Domain.Entities;
using BodegaApp.Models;

namespace BodegaApp.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly BodegaDbContext _context;

        public SuppliersController(BodegaDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin => User.IsInRole(Roles.Admin);

        // Un proveedor inactivo es un proveedor "borrado": solo el admin lo sigue viendo
        private IQueryable<Supplier> VisibleSuppliers() =>
            IsAdmin ? _context.Suppliers : _context.Suppliers.Where(s => s.IsActive);

        [HttpGet]
        public async Task<IActionResult> Index(bool includeInactive = false)
        {
            // El filtro de inactivos es cosa del admin
            includeInactive = includeInactive && IsAdmin;

            var query = _context.Suppliers.AsNoTracking().AsQueryable();

            if (!includeInactive)
                query = query.Where(s => s.IsActive);

            ViewData["IncludeInactive"] = includeInactive;

            var suppliers = await query.ToListAsync();
            return View(suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await VisibleSuppliers()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SupplierFormViewModel());
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Add(new Supplier
            {
                Name = model.Name,
                Contact = model.Contact,
                Type = model.Type,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                IsActive = model.IsActive
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Suppliers/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await VisibleSuppliers()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();

            return View(ToViewModel(supplier));
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid) return View(model);

            // Se copian los campos sobre la entidad guardada, sin escribir columnas de más
            var supplier = await VisibleSuppliers().FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();

            supplier.Name = model.Name;
            supplier.Contact = model.Contact;
            supplier.Type = model.Type;
            supplier.Phone = model.Phone;
            supplier.Email = model.Email;
            supplier.Address = model.Address;
            supplier.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Suppliers/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await VisibleSuppliers()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await VisibleSuppliers().FirstOrDefaultAsync(s => s.Id == id);
            if (supplier != null)
            {
                supplier.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Suppliers/HardDelete/5
        [Authorize(Roles = Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> HardDelete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/HardDelete/5
        [Authorize(Roles = Roles.Admin)]
        [HttpPost, ActionName("HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private static SupplierFormViewModel ToViewModel(Supplier supplier) => new()
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
    }
}
