using AutoHub.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarBrand> CarBrands => Set<CarBrand>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CarBrand>().HasIndex(b => b.Name).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<SystemSetting>().HasIndex(s => s.Key).IsUnique();
        modelBuilder.Entity<Car>().Property(c => c.Price).HasPrecision(12, 2);
        modelBuilder.Entity<Car>().HasOne(c => c.CarBrand).WithMany(b => b.Cars)
            .HasForeignKey(c => c.CarBrandId).OnDelete(DeleteBehavior.Restrict);
    }
}
