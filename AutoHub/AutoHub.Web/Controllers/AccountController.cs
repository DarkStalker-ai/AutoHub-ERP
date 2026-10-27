using AutoHub.Web.Data;
using AutoHub.Web.Models;
using AutoHub.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoHub.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly AuditService _audit;
    private readonly PasswordHasher<AppUser> _hasher = new();
    public AccountController(ApplicationDbContext db, AuditService audit) { _db = db; _audit = audit; }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var user = await _db.AppUsers.SingleOrDefaultAsync(u => u.Username == username && u.IsActive);
        if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
        {
            await _audit.WriteAsync("LoginFailed", "AppUser", username, "Failed login attempt", username);
            ModelState.AddModelError(string.Empty, "Wrong username or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        var claims = new List<Claim> { new(ClaimTypes.Name, user.Username), new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Role, user.Role) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        await _audit.WriteAsync("Login", "AppUser", user.Id.ToString(), "Successful login", user.Username);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "unknown";
        await _audit.WriteAsync("Logout", "AppUser", username, "Logout", username);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
