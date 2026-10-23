using Microsoft.AspNetCore.Mvc;

namespace ZTP_Lab01.Controllers;

public class ProductController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}