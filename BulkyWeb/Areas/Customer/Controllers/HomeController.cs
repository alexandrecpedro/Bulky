using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting product display...");

        IEnumerable<Product> productList = await _unitOfWork.Product.GetAll(page: page, pageSize: pageSize, includeProperties: "Category");
        return View(productList);
    }

    public async Task<IActionResult> Details(int productId)
    {
        _logger.LogInformation("Starting details display...");

        Product? product = await _unitOfWork.Product.Get(filter: u => u.Id == productId, includeProperties: "Category");

        if (product == null)
        {
            return NotFound();
        }

        ShoppingCart cart = new()
        {
            Product = product,
            Count = 1,
            ProductId = productId
        };

        return View(cart);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Details(ShoppingCart shoppingCart)
    {
        _logger.LogInformation("Starting shopping cart add product...");

        // Get logged user
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found.");
            return Unauthorized();
        }

        shoppingCart.ApplicationUserId = userId;

        Product? product = await _unitOfWork.Product.Get(filter: u => u.Id == shoppingCart.ProductId, includeProperties: "Category");

        if (product == null)
        {
            _logger.LogWarning($"Product with ID {shoppingCart.ProductId} not found.");
            return NotFound();
        }

        Category? category = await _unitOfWork.Category.Get(filter: u => u.Id == product.CategoryId);
        if (category == null)
        {
            _logger.LogWarning($"Category with ID {product.CategoryId} not found.");
            return NotFound();
        }

        ShoppingCart? cartFromDb = await _unitOfWork.ShoppingCart.Get(filter: u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

        if (cartFromDb != null)
        {
            //shoppingCart cart exists
            _logger.LogInformation($"Updating existing shopping cart for user {userId} with Product ID {shoppingCart.ProductId}.");
            cartFromDb.Count += shoppingCart.Count;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
        }
        else
        {
            _logger.LogInformation($"Adding new shopping cart for user {userId} with Product ID {shoppingCart.ProductId}.");
            await _unitOfWork.ShoppingCart.Add(shoppingCart);
        }

        await _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
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
