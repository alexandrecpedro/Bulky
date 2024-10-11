using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Bulky.Utility.Messages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.ViewComponents;

public class ShoppingCartViewComponent : ViewComponent
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ViewComponent> _logger;

    public ShoppingCartViewComponent(IUnitOfWork unitOfWork, ILogger<ViewComponent> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        string? claim = GetLoggedUserId();
        await SetSession(userId: claim);
        var sessionCartContext = HttpContext.Session.GetInt32(key: SD.SessionCart) ?? 0;
        return View(sessionCartContext);
    }

    #region PRIVATE METHODS
    private string? GetLoggedUserId()
    {
        // Get logged user
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError(message: LogExceptionMessages.UserIdNotFoundException);
            return null;
        }

        return userId;
    }

    private async Task SetSession(string? userId)
    {
        if (userId is null)
        {
            HttpContext.Session.Clear();
            return;
        }

        var count = 0;

        IEnumerable<ShoppingCart>? shoppingCarts = await _unitOfWork.ShoppingCart.GetAll(filter: u => u.ApplicationUserId == userId);
        if (shoppingCarts is not null && shoppingCarts.Any())
            count = shoppingCarts.Count();

        HttpContext.Session.SetInt32(key: SD.SessionCart, value: count);
    }

    #endregion
}
