using System.Diagnostics;
using AutoHub.Web.Data;
using AutoHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db) { _logger = logger; _db = db; }
    public async Task<IActionResult> Index()
    {
        ViewBag.CustomerCount = await _db.Customers.CountAsync();
        ViewBag.CarCount = await _db.Cars.CountAsync();
        ViewBag.AvailableCount = await _db.Cars.CountAsync(c => c.Status == "Available");
        return View();
    }
    public IActionResult Privacy() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
