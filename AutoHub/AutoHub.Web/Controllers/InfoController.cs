using Microsoft.AspNetCore.Mvc;
namespace AutoHub.Web.Controllers;
public class InfoController : Controller
{
    public IActionResult Help() => View();
    public IActionResult About() => View();
}
