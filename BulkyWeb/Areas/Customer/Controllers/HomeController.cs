using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Starting product display...");

        IEnumerable<Product> productList = await _unitOfWork.Product.GetAll(includeProperties: "Category");
        return View(productList);
    }

    public async Task<IActionResult> Details(int productId)
    {
        _logger.LogInformation("Starting product details display...");

        Product? product = await _unitOfWork.Product.Get(filter: u => u.Id == productId, includeProperties: "Category");

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Starting privacy search...");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
