using System.ComponentModel.DataAnnotations;

namespace AutoHub.Web.Models;

public class AppUser
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string Username { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Role { get; set; } = "Manager";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
