using System.Diagnostics;
using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DB db;

    public HomeController(ILogger<HomeController> logger, DB db)
    {
        _logger = logger;
        this.db = db;
    }

    public IActionResult Index()
    {
        var products = db.products.Where((x) => !x.IsDeleted).ToList();
        return View(products);
    }

    public IActionResult Privacy() { return View(); }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None,
                   NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ??
                                                     HttpContext.TraceIdentifier
        });
    }
}
