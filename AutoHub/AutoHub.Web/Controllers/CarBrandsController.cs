using Microsoft.AspNetCore.Authorization;
using AutoHub.Web.Data;
using AutoHub.Web.Services;
using AutoHub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Controllers
{
    [Authorize]
public class CarBrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        public CarBrandsController(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // GET: CarBrands
        public async Task<IActionResult> Index()
        {
            var brands = await _context.CarBrands
                .OrderBy(b => b.Name)
                .ToListAsync();
            await _audit.WriteAsync("ViewTable", "CarBrand", null, "Перегляд довідника марок");
            return View(brands);
        }

        // GET: CarBrands/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.CarBrands
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null)
                return NotFound();

            return View(brand);
        }

        // GET: CarBrands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CarBrands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarBrand brand)
        {
            if (await _context.CarBrands
                .AnyAsync(b => b.Name == brand.Name))
            {
                ModelState.AddModelError(
                    nameof(brand.Name),
                    "A brand with the same name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.CarBrands.Add(brand);
                await _context.SaveChangesAsync();
                await _audit.WriteAsync("Create", "CarBrand", brand.Id.ToString(), $"Додано марку {brand.Name}");

                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }

        // GET: CarBrands/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.CarBrands.FindAsync(id);

            if (brand == null)
                return NotFound();

            return View(brand);
        }

        // POST: CarBrands/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarBrand brand)
        {
            if (id != brand.Id)
                return NotFound();

            if (await _context.CarBrands.AnyAsync(
                b => b.Name == brand.Name && b.Id != brand.Id))
            {
                ModelState.AddModelError(
                    nameof(brand.Name),
                    "A brand with the same name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(brand);
                await _context.SaveChangesAsync();
                await _audit.WriteAsync("Update", "CarBrand", brand.Id.ToString(), $"Змінено марку {brand.Name}");

                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }

        // GET: CarBrands/Delete/1
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.CarBrands
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null)
                return NotFound();

            return View(brand);
        }

        // POST: CarBrands/Delete/1
        [HttpPost, Authorize(Roles = "Admin"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.CarBrands.FindAsync(id);

            if (brand == null)
                return NotFound();

            bool hasCars = await _context.Cars
                .AnyAsync(c => c.CarBrandId == id);

            if (hasCars)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to delete the brand because it is associated with existing cars.");

                return View(brand);
            }

            _context.CarBrands.Remove(brand);
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("Delete", "CarBrand", id.ToString(), $"Видалено марку {brand.Name}");

            return RedirectToAction(nameof(Index));
        }
    }
}