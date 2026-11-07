using Microsoft.AspNetCore.Mvc;

namespace SecurityStoreImport.Controllers;

public class HomeController : Controller
{
    /// <summary>Главная страница — сразу список товаров.</summary>
    public IActionResult Index()
    {
        return RedirectToAction(nameof(ProductsController.Index), "Products");
    }

    public IActionResult Error()
    {
        return View();
    }
}
