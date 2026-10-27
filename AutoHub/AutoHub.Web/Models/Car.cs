
using System.ComponentModel.DataAnnotations;

namespace AutoHub.Web.Models;

public class Car
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Model { get; set; } = string.Empty;

    public int CarBrandId { get; set; }

    public CarBrand? CarBrand { get; set; }

    public int ProductionYear { get; set; }

    [StringLength(30)]
    public string? Color { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "Available";

    public decimal Price { get; set; }
}