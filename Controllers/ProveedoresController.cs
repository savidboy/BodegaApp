using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BodegaApp.Data;
using BodegaApp.Domain.Entities;

namespace BodegaApp.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly BodegaDbContext _context;

        public ProveedoresController(BodegaDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CREAR PROVEEDOR (GET y POST)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proveedor);
                await _context.SaveChangesAsync(); // Guarda los cambios del nuevo proveedor en la base de datos
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // ==========================================
        // 2. EDITAR PROVEEDOR (GET y POST)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound(); // Muestra error si el ID no existe

            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proveedor);
                    await _context.SaveChangesAsync(); // Actualiza la información del proveedor en la base de datos[cite: 1]
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Proveedores.Any(e => e.Id == proveedor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // ==========================================
        // 3. ELIMINAR PROVEEDOR (GET y POST)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proveedor == null) return NotFound();

            return View(proveedor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor != null)
            {
                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync(); // Elimina el registro del proveedor de la base de datos[cite: 1]
            }

            return RedirectToAction(nameof(Index));
        }
    }
}