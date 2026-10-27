using Microsoft.AspNetCore.Authorization;

using AutoHub.Web.Data;
using AutoHub.Web.Services;
using AutoHub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Controllers;

[Authorize]
public class CarsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public CarsController(ApplicationDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    // GET: /Cars
    public async Task<IActionResult> Index(string? q, string? status, int? brandId, string sort = "model")
    {
        var query = _context.Cars.Include(c => c.CarBrand).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Model.ToLower().Contains(q.ToLower()) || (c.CarBrand != null && c.CarBrand.Name.ToLower().Contains(q.ToLower())) || (c.Color ?? "").ToLower().Contains(q.ToLower()));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);
        if (brandId.HasValue) query = query.Where(c => c.CarBrandId == brandId.Value);
        query = sort switch { "price" => query.OrderBy(c => c.Price), "year" => query.OrderByDescending(c => c.ProductionYear), "brand" => query.OrderBy(c => c.CarBrand!.Name), _ => query.OrderBy(c => c.Model) };
        ViewBag.Q = q; ViewBag.Status = status; ViewBag.Sort = sort;
        ViewBag.Brands = new SelectList(await _context.CarBrands.AsNoTracking().OrderBy(b => b.Name).ToListAsync(), "Id", "Name", brandId);
        var result = await query.ToListAsync();
        await _audit.WriteAsync("ViewTable", "Car", null, "Перегляд таблиці автомобілів");
        return View(result);
    }

    // GET: /Cars/Details/1
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var car = await _context.Cars
            .Include(c => c.CarBrand)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
            return NotFound();

        return View(car);
    }

    // GET: /Cars/Create
    public async Task<IActionResult> Create()
    {
        await LoadBrandsAsync();
        return View();
    }

    // POST: /Cars/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Car car)
    {
        if (!await _context.CarBrands
                .AnyAsync(b => b.Id == car.CarBrandId))
        {
            ModelState.AddModelError(
                nameof(car.CarBrandId),
                "Оберіть наявний бренд.");
        }

        if (!ModelState.IsValid)
        {
            await LoadBrandsAsync(car.CarBrandId);
            return View(car);
        }

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        await _audit.WriteAsync("Create", "Car", car.Id.ToString(), $"Додано автомобіль {car.Model}");

        return RedirectToAction(nameof(Index));
    }

    // GET: /Cars/Edit/1
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var car = await _context.Cars.FindAsync(id);

        if (car == null)
            return NotFound();

        await LoadBrandsAsync(car.CarBrandId);
        return View(car);
    }

    // POST: /Cars/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Car car)
    {
        if (id != car.Id)
            return NotFound();

        if (!await _context.CarBrands
                .AnyAsync(b => b.Id == car.CarBrandId))
        {
            ModelState.AddModelError(
                nameof(car.CarBrandId),
                "Choose available brand.");
        }

        if (!ModelState.IsValid)
        {
            await LoadBrandsAsync(car.CarBrandId);
            return View(car);
        }

        var existingCar = await _context.Cars.FindAsync(id);

        if (existingCar == null)
            return NotFound();

        existingCar.Model = car.Model;
        existingCar.CarBrandId = car.CarBrandId;
        existingCar.ProductionYear = car.ProductionYear;
        existingCar.Color = car.Color;
        existingCar.Status = car.Status;
        existingCar.Price = car.Price;

        await _context.SaveChangesAsync();
        await _audit.WriteAsync("Update", "Car", id.ToString(), $"Змінено автомобіль {existingCar.Model}");

        return RedirectToAction(nameof(Index));
    }

    // GET: /Cars/Delete/1
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var car = await _context.Cars
            .Include(c => c.CarBrand)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
            return NotFound();

        return View(car);
    }

    // POST: /Cars/Delete/1
    [HttpPost, Authorize(Roles = "Admin"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var car = await _context.Cars.FindAsync(id);

        if (car != null)
        {
            var deletedModel = car.Model;
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            await _audit.WriteAsync("Delete", "Car", id.ToString(), $"Видалено автомобіль {deletedModel}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadBrandsAsync(int? selectedId = null)
    {
        ViewBag.CarBrandId = new SelectList(
            await _context.CarBrands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .ToListAsync(),
            "Id",
            "Name",
            selectedId);
    }
}
