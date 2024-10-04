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

        var userId = GetLoggedUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var shoppingCartList = await _unitOfWork.ShoppingCart.GetAll(
            filter: u => u.ApplicationUserId == userId, 
            page: page, 
            pageSize: pageSize, 
            includeProperties: "Product");

        ShoppingCartVM = new()
        {
            ShoppingCartList = shoppingCartList,
            OrderHeader = new()
        };

        foreach(var cart in ShoppingCartVM.ShoppingCartList)
        {
            cart.Price = GetPriceBasedOnQuantity(shoppingCart: cart);
            ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
        }

        return View(ShoppingCartVM);
    }

    public async Task<IActionResult> Summary(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting shopping cart summarizing...");

        var userId = GetLoggedUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _unitOfWork.ApplicationUser.Get(filter: u => u.Id == userId);
        if (user is null)
        {
            _logger.LogWarning("User not found!");
            return Unauthorized();
        }

        var shoppingCartList = await _unitOfWork.ShoppingCart.GetAll(
            filter: u => u.ApplicationUserId == userId, 
            page: page, 
            pageSize: pageSize, 
            includeProperties: "Product");

        if (shoppingCartList is null || !shoppingCartList.Any())
        {
            _logger.LogError($"Shopping cart not found!");
            return NotFound();
        }

        double orderTotal = shoppingCartList.Aggregate(0d, (total, cart) =>
        {
            cart.Price = GetPriceBasedOnQuantity(shoppingCart: cart);
            return total + (cart.Price * cart.Count);
        });

        OrderHeader orderHeader = new()
        {
            ApplicationUser = user,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            StreetAddress = user.StreetAddress ?? string.Empty,
            City = user.City ?? string.Empty,
            State = user.State ?? string.Empty,
            PostalCode = user.PostalCode ?? string.Empty,
            OrderTotal = orderTotal
        };

        ShoppingCartVM = new()
        {
            ShoppingCartList = shoppingCartList,
            OrderHeader = orderHeader,
        };

        return View(ShoppingCartVM);
    }

    private string? GetLoggedUserId()
    {
        // Get logged user
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found!");
            return null;
        }

        return userId;
    }

    public async Task<IActionResult> Plus(int cartId)
    {
        _logger.LogInformation("Starting add product quantity to shopping cart...");
        var cartFromDb = await _unitOfWork.ShoppingCart.Get(filter: u => u.Id == cartId);

        if (cartFromDb is null)
        {
            _logger.LogError($"Shopping cart ID {cartId} not found!");
            return NotFound();
        }

        cartFromDb.Count += 1;
        _unitOfWork.ShoppingCart.Update(cartFromDb);
        
        TempData["success"] = "Cart updated successfully!";
        
        await _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Minus(int cartId)
    {
        _logger.LogInformation("Starting reducing product quantity from shopping cart...");
        var cartFromDb = await _unitOfWork.ShoppingCart.Get(filter: u => u.Id == cartId);

        if (cartFromDb is null)
        {
            _logger.LogError($"Shopping cart ID {cartId} not found!");
            return NotFound();
        }

        if (cartFromDb.Count <= 1)
        {
            return await Remove(cartId: cartId);
        }

        cartFromDb.Count -= 1;
        _unitOfWork.ShoppingCart.Update(cartFromDb);

        TempData["success"] = "Cart updated successfully!";

        await _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(int cartId)
    {
        _logger.LogInformation($"Starting removing product from shopping cart...");

        var cartFromDb = await _unitOfWork.ShoppingCart.Get(filter: u => u.Id == cartId);

        if (cartFromDb is null)
        {
            _logger.LogError($"Shopping cart ID {cartId} not found!");
            return NotFound();
        }

        _unitOfWork.ShoppingCart.Remove(cartFromDb);

        TempData["success"] = "Cart removed successfully!";

        await _unitOfWork.Save();
        return RedirectToAction(nameof(Index));
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
