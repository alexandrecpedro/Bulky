using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public CartController(ILogger<CartController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation($"Starting shopping cart display content...");

        // Get logged user
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found!");
            return Unauthorized();
        }

        var shoppingCartList = await _unitOfWork.ShoppingCart.GetAll(filter: u => u.ApplicationUserId == userId, page: page, pageSize: pageSize, includeProperties: "Product");

        ShoppingCartVM = new()
        {
            ShoppingCartList = shoppingCartList
        };

        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(shoppingCart: cart);
            ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
    {
        _logger.LogInformation("Starting find product price based on quantity...");

        return shoppingCart.Count switch
        {
            <= 50 => shoppingCart.Product.Price,
            <= 100 => shoppingCart.Product.Price50,
            _ => shoppingCart.Product.Price100,
        };
    }
}
