using AutoHub.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        if (!await db.AppUsers.AnyAsync())
        {
            var username = config["BootstrapAdmin:Username"] ?? "admin";
            var password = config["BootstrapAdmin:Password"] ?? "ChangeMe123!";
            var user = new AppUser { Username = username, Role = "Admin", IsActive = true };
            user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
            db.AppUsers.Add(user);
        }
        var defaults = new[] {
            new SystemSetting { Key = "CompanyName", Value = "AutoHub ERP", Description = "Organization Name" },
            new SystemSetting { Key = "Currency", Value = "UAH", Description = "Accounting Currency" },
            new SystemSetting { Key = "ItemsPerPage", Value = "20", Description = "Number of records per page" }
        };
        foreach (var setting in defaults)
            if (!await db.SystemSettings.AnyAsync(s => s.Key == setting.Key)) db.SystemSettings.Add(setting);
        if (!await db.CarBrands.AnyAsync())
            db.CarBrands.AddRange(new CarBrand { Name = "Toyota" }, new CarBrand { Name = "Volkswagen" }, new CarBrand { Name = "BMW" });
        await db.SaveChangesAsync();
    }
}
