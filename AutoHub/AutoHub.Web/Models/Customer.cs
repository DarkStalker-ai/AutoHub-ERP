using System.ComponentModel.DataAnnotations;

namespace AutoHub.Web.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }
    
    [StringLength(250)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
