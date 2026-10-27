
using System.ComponentModel.DataAnnotations;

namespace AutoHub.Web.Models;

public class CarBrand
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
