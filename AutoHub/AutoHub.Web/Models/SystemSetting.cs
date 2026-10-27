using System.ComponentModel.DataAnnotations;

namespace AutoHub.Web.Models;

public class SystemSetting
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Key { get; set; } = string.Empty;
    [StringLength(1000)] public string? Value { get; set; }
    [StringLength(300)] public string? Description { get; set; }
}
