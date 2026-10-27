using AutoHub.Web.Data;
using AutoHub.Web.Models;
using AutoHub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly PasswordHasher<AppUser> _hasher = new();
    public AdminController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    public async Task<IActionResult> Index() => View(new AdminDashboardViewModel
    {
        Users = await _db.AppUsers.AsNoTracking().CountAsync(),
        Logs = await _db.AuditLogs.AsNoTracking().CountAsync(),
        Settings = await _db.SystemSettings.AsNoTracking().CountAsync()
    });

    public async Task<IActionResult> Users() => View(await _db.AppUsers.AsNoTracking().OrderBy(u => u.Username).ToListAsync());

    [HttpGet]
    public IActionResult CreateUser() => View(new CreateUserViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (model.Role is not ("Admin" or "Manager")) ModelState.AddModelError(nameof(model.Role), "Unknown role.");
        if (await _db.AppUsers.AnyAsync(u => u.Username == model.Username)) ModelState.AddModelError(nameof(model.Username), "Username already exists.");
        if (!ModelState.IsValid) return View(model);
        var user = new AppUser { Username = model.Username.Trim(), Role = model.Role, IsActive = true };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        _db.AppUsers.Add(user); await _db.SaveChangesAsync();
        await _audit.WriteAsync("UserCreated", "AppUser", user.Id.ToString(), $"Created user with role {user.Role}");
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(int id)
    {
        var user = await _db.AppUsers.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Username == User.Identity!.Name) { TempData["Error"] = "You cannot deactivate your own account."; return RedirectToAction(nameof(Users)); }
        user.IsActive = !user.IsActive; await _db.SaveChangesAsync();
        await _audit.WriteAsync("UserStatusChanged", "AppUser", id.ToString(), $"IsActive={user.IsActive}");
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Logs(string? q, string? actionType, DateTime? from, DateTime? to)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Username.ToLower().Contains(q.ToLower()) || (x.EntityName ?? "").ToLower().Contains(q.ToLower()) || (x.Details ?? "").ToLower().Contains(q.ToLower()));
        if (!string.IsNullOrWhiteSpace(actionType)) query = query.Where(x => x.Action == actionType);
        if (from.HasValue) query = query.Where(x => x.OccurredAtUtc >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));
        if (to.HasValue) query = query.Where(x => x.OccurredAtUtc < DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Utc));
        ViewBag.Q = q; ViewBag.ActionType = actionType; ViewBag.From = from?.ToString("yyyy-MM-dd"); ViewBag.To = to?.ToString("yyyy-MM-dd");
        return View(await query.OrderByDescending(x => x.OccurredAtUtc).Take(500).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Settings() => View(await _db.SystemSettings.OrderBy(s => s.Key).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(List<SystemSetting> settings)
    {
        foreach (var item in settings)
        {
            var stored = await _db.SystemSettings.FindAsync(item.Id);
            if (stored != null) stored.Value = item.Value?.Trim();
        }
        await _db.SaveChangesAsync();
        await _audit.WriteAsync("SettingsUpdated", "SystemSetting", null, "Updated system settings");
        TempData["Message"] = "Settings saved.";
        return RedirectToAction(nameof(Settings));
    }
}

public class AdminDashboardViewModel { public int Users { get; set; } public int Logs { get; set; } public int Settings { get; set; } }
public class CreateUserViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(60, MinimumLength = 3)] public string Username { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8)] public string Password { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required] public string Role { get; set; } = "Manager";
}
