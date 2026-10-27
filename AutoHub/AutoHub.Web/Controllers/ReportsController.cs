using AutoHub.Web.Data;
using AutoHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    public ReportsController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }
    public async Task<IActionResult> Index()
    {
        var cars = await _db.Cars.AsNoTracking().Include(c => c.CarBrand).ToListAsync();
        var vm = new ReportsViewModel {
            CustomerCount = await _db.Customers.CountAsync(),
            CarCount = cars.Count,
            AvailableCount = cars.Count(c => c.Status == "Available"),
            SoldCount = cars.Count(c => c.Status == "Sold"),
            InventoryValue = cars.Where(c => c.Status != "Sold").Sum(c => c.Price),
            ByBrand = cars.GroupBy(c => c.CarBrand?.Name ?? "Unknown brand").Select(g => new BrandStat { Brand = g.Key, Count = g.Count(), Value = g.Sum(x => x.Price) }).OrderByDescending(x => x.Count).ToList()
        };
        await _audit.WriteAsync("ReportGenerated", "Report", "InventorySummary", "A report has been drawn up");
        return View(vm);
    }
}
public class ReportsViewModel { public int CustomerCount { get; set; } public int CarCount { get; set; } public int AvailableCount { get; set; } public int SoldCount { get; set; } public decimal InventoryValue { get; set; } public List<BrandStat> ByBrand { get; set; } = new(); }
public class BrandStat { public string Brand { get; set; } = ""; public int Count { get; set; } public decimal Value { get; set; } }
