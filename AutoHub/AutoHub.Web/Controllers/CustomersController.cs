using AutoHub.Web.Data;
using AutoHub.Web.Models;
using AutoHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;
    public CustomersController(ApplicationDbContext context, AuditService audit) { _context = context; _audit = audit; }

    public async Task<IActionResult> Index(string? q, string sort = "name")
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.FullName.ToLower().Contains(q.ToLower()) || (c.PhoneNumber ?? "").ToLower().Contains(q.ToLower()) || (c.Email ?? "").ToLower().Contains(q.ToLower()));
        query = sort switch { "email" => query.OrderBy(c => c.Email), "created" => query.OrderByDescending(c => c.CreatedAt), _ => query.OrderBy(c => c.FullName) };
        ViewBag.Q = q; ViewBag.Sort = sort;
        var result = await query.ToListAsync();
        await _audit.WriteAsync("ViewTable", "Customer", null, "Перегляд таблиці клієнтів");
        return View(result);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,PhoneNumber,Email,Address")] Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);
        customer.CreatedAt = DateTime.UtcNow;
        _context.Customers.Add(customer); await _context.SaveChangesAsync();
        await _audit.WriteAsync("Create", "Customer", customer.Id.ToString(), $"Додано клієнта: {customer.FullName}");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var customer = await _context.Customers.FindAsync(id);
        return customer == null ? NotFound() : View(customer);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,PhoneNumber,Email,Address")] Customer input)
    {
        if (id != input.Id) return NotFound();
        if (!ModelState.IsValid) return View(input);
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        customer.FullName = input.FullName; customer.PhoneNumber = input.PhoneNumber; customer.Email = input.Email; customer.Address = input.Address;
        await _context.SaveChangesAsync();
        await _audit.WriteAsync("Update", "Customer", id.ToString(), $"Змінено дані клієнта: {customer.FullName}");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return customer == null ? NotFound() : View(customer);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        var name = customer.FullName; _context.Customers.Remove(customer); await _context.SaveChangesAsync();
        await _audit.WriteAsync("Delete", "Customer", id.ToString(), $"Видалено клієнта: {name}");
        return RedirectToAction(nameof(Index));
    }
}
