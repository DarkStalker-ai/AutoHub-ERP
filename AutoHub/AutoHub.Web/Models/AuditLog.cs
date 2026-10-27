using System.ComponentModel.DataAnnotations;

namespace AutoHub.Web.Models;

public class AuditLog
{
    public long Id { get; set; }
    [StringLength(100)] public string Username { get; set; } = "anonymous";
    [Required, StringLength(100)] public string Action { get; set; } = string.Empty;
    [StringLength(100)] public string? EntityName { get; set; }
    [StringLength(100)] public string? EntityId { get; set; }
    [StringLength(1000)] public string? Details { get; set; }
    [StringLength(80)] public string? IpAddress { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
