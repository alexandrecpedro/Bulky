using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Bulky.Utility.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Globalization;
using System.Net;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly ILogger<CartController> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailSender _emailSender;
    [BindProperty]
    public ShoppingCartVM ShoppingCartVM { get; set; }

    public CartController(ILogger<CartController> logger, IUnitOfWork unitOfWork, IEmailSender emailSender)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation($"Starting shopping cart display content...");

        string? userId = GetLoggedUserId();
        if (userId is null) return Unauthorized();

        var shoppingCartList = await GetShoppingCartList(
            applicationUserId: userId,
            page: page,
            pageSize: pageSize
        );

        var orderTotal = CalculateOrderTotal(shoppingCartList: shoppingCartList);
        ShoppingCartVM = new()
        {
            ShoppingCartList = shoppingCartList,
            OrderHeader = new() { OrderTotal = orderTotal }
        };

        return View(ShoppingCartVM);
    }

    public async Task<IActionResult> Summary(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting shopping cart summarizing...");

        string? userId = GetLoggedUserId();
        if (userId is null) return Unauthorized();

        ApplicationUser? user = await GetUserById(applicationUserId: userId);
        if (user is null) return Unauthorized();

        var shoppingCartList = await GetShoppingCartList(
            applicationUserId: userId,
            page: page,
            pageSize: pageSize
        );
        if (shoppingCartList is null) return NotFound();

        OrderHeader orderHeader = CreateOrderHeader(applicationUser: user, shoppingCartList: shoppingCartList);

        ShoppingCartVM = new()
        {
            ShoppingCartList = shoppingCartList,
            OrderHeader = orderHeader,
        };

        return View(ShoppingCartVM);
    }

    [HttpPost]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPOST(int? page, int? pageSize)
    {
        _logger.LogInformation("Starting placing an order...");

        var userId = GetLoggedUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await GetUserById(applicationUserId: userId);
        if (user is null) return Unauthorized();

        var shoppingCartList = await GetShoppingCartList(
            applicationUserId: userId, 
            page: page, 
            pageSize: pageSize
        );

        if (shoppingCartList is null) return NotFound();

        PrepareOrderHeader(applicationUserId: userId, shoppingCartList: shoppingCartList);

        SetOrderStatus(applicationUser: user);

        await _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
        await _unitOfWork.Save();

        await AddOrderDetails(shoppingCartList: shoppingCartList);

        if (!IsCompanyUser(applicationUser: user))
            // stripe logic
            return await CreatePaymentSession(shoppingCartVM: ShoppingCartVM);

        return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
    }

    public async Task<IActionResult> OrderConfirmation(int id, int page = 1, int pageSize = 10)
    {
        _logger.LogInformation($"Starting confirm order ID {id}");

        OrderHeader? orderHeader = await GetOrderHeader(orderId: id);

        if (orderHeader is null) return NotFound();

        if (!IsCompanyUser(applicationUser: orderHeader.ApplicationUser) && orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            await HandlePaymentStatus(orderHeader: orderHeader);


        var userId = GetLoggedUserId();
        if (string.IsNullOrWhiteSpace(userId)) return NotFound();

        //await _emailSender.SendEmailAsync(
        //    email: orderHeader.ApplicationUser.Email ?? string.Empty,
        //    subject: "New order - Bulky Book",
        //    htmlMessage: $"<p>New order created - {orderHeader.Id}</p>"
        //);
        
        IEnumerable<ShoppingCart>? shoppingCartList = await GetShoppingCartList(
            applicationUserId: userId,
            page: page,
            pageSize: pageSize
        );
        if (shoppingCartList is null) return NotFound();

        _unitOfWork.ShoppingCart.RemoveRange(entity: shoppingCartList);
        await _unitOfWork.Save();

        return View(id);
    }

    public async Task<IActionResult> Plus(int cartId)
    {
        _logger.LogInformation("Starting add product quantity to shopping cart...");
        var cartFromDb = await _unitOfWork.ShoppingCart.Get(filter: u => u.Id == cartId);

        if (cartFromDb is null)
        {
            _logger.LogError(message: LogExceptionMessages.ShoppingCartIdNotFoundException);
            return NotFound();
        }

        cartFromDb.Count += 1;
        _unitOfWork.ShoppingCart.Update(cartFromDb);

        TempData["success"] = SuccessDataMessages.ShoppingCartUpdatedSuccess;

        await _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Minus(int cartId)
    {
        _logger.LogInformation("Starting reducing product quantity from shopping cart...");
        var cartFromDb = await _unitOfWork.ShoppingCart.Get(
            filter: u => u.Id == cartId,
            tracked: true
        );

        if (cartFromDb is null)
        {
            _logger.LogError(message: LogExceptionMessages.ShoppingCartNotFoundException);
            return NotFound();
        }

        if (cartFromDb.Count <= 1)
        {
            return await Remove(cartId: cartId);
        }

        cartFromDb.Count -= 1;
        _unitOfWork.ShoppingCart.Update(cartFromDb);

        TempData["success"] = SuccessDataMessages.ShoppingCartUpdatedSuccess;

        await _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(int cartId)
    {
        _logger.LogInformation($"Starting removing product from shopping cart...");

        var cartFromDb = await _unitOfWork.ShoppingCart.Get(
            filter: u => u.Id == cartId,
            tracked: true
        );

        if (cartFromDb is null)
        {
            _logger.LogError(message: LogExceptionMessages.ShoppingCartNotFoundException);
            return NotFound();
        }

        _unitOfWork.ShoppingCart.Remove(cartFromDb);
        await _unitOfWork.Save();

        TempData["success"] = SuccessDataMessages.ShoppingCartDeletedSuccess;

        await SetSession(userId: cartFromDb.ApplicationUserId);
        return RedirectToAction(nameof(Index));
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

    private async Task<ApplicationUser?> GetUserById(string applicationUserId)
    {
        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            _logger.LogError(message: LogExceptionMessages.ShoppingCartIdNotFoundException);
            return null;
        }
        
        var user = await _unitOfWork.ApplicationUser.Get(filter: u => u.Id == applicationUserId);
        if (user is null)
        {
            _logger.LogError(message: LogExceptionMessages.UserNotFoundException);
            return null;
        }

        return user;
    }

    private async Task<IEnumerable<ShoppingCart>> GetShoppingCartList(string applicationUserId, int? page, int? pageSize)
    {
        var shoppingCarts = await _unitOfWork.ShoppingCart.GetAll(
            filter: u => u.ApplicationUserId == applicationUserId,
            page: page ?? 1,
            pageSize: pageSize ?? 10,
            includeProperties: "Product");

        if (shoppingCarts is null || !shoppingCarts.Any())
        {
            _logger.LogError(message: LogExceptionMessages.ShoppingCartNotFoundException);
            return [];
        }

        return shoppingCarts.Select(cart =>
        {
            cart.Price = GetPriceBasedOnQuantity(shoppingCart: cart);
            return cart;
        });
    }

    private double CalculateOrderTotal(IEnumerable<ShoppingCart>? shoppingCartList)
    {
        if (shoppingCartList is null || !shoppingCartList.Any())
        {
            return 0d;
        }
        return shoppingCartList.Sum(cart => GetPriceBasedOnQuantity(cart) * cart.Count);
    }

    private OrderHeader CreateOrderHeader(ApplicationUser applicationUser, IEnumerable<ShoppingCart> shoppingCartList)
    {
        var orderTotal = CalculateOrderTotal(shoppingCartList: shoppingCartList);

        return new OrderHeader
        {
            ApplicationUser = applicationUser,
            Name = applicationUser.Name,
            PhoneNumber = applicationUser.PhoneNumber ?? string.Empty,
            StreetAddress = applicationUser.StreetAddress ?? string.Empty,
            City = applicationUser.City ?? string.Empty,
            State = applicationUser.State ?? string.Empty,
            PostalCode = applicationUser.PostalCode ?? string.Empty,
            OrderTotal = orderTotal
        };
    }

    private void PrepareOrderHeader(string applicationUserId, IEnumerable<ShoppingCart> shoppingCartList)
    {
        ShoppingCartVM.ShoppingCartList = shoppingCartList;
        ShoppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
        ShoppingCartVM.OrderHeader.ApplicationUserId = applicationUserId;

        ShoppingCartVM.OrderHeader.OrderTotal = shoppingCartList.Aggregate(0d, (total, cart) =>
        {
            cart.Price = GetPriceBasedOnQuantity(shoppingCart: cart);
            return total + (cart.Price * cart.Count);
        });
    }

    private void SetOrderStatus(ApplicationUser applicationUser)
    {
        bool isCompanyUser = IsCompanyUser(applicationUser: applicationUser);
        var statusMapping = new Dictionary<bool, (string PaymentStatus, string OrderStatus)>
        {
            // regular customer account - capture payment
            [false] = OrderStatusManagement.orderStatusCustomer[OrderStatusManagement.Makes_Payment],

            // company user
            [true] = OrderStatusManagement.orderStatusCompany[OrderStatusManagement.Order_Confirmation]
        };

        (ShoppingCartVM.OrderHeader.PaymentStatus, ShoppingCartVM.OrderHeader.OrderStatus) = statusMapping[isCompanyUser];
    }

    private bool IsCompanyUser(ApplicationUser applicationUser)
    {
        return applicationUser.CompanyId.HasValue && applicationUser.CompanyId.Value > 0;
    }

    private async Task AddOrderDetails(IEnumerable<ShoppingCart> shoppingCartList)
    {
        var orderDetails = ShoppingCartVM.ShoppingCartList
            .Select(cart => new OrderDetail
            {
                ProductId = cart.ProductId,
                OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                Price = cart.Price,
                Count = cart.Count,
            })
            .ToList();

        await Task.WhenAll(orderDetails.Select(orderDetail => _unitOfWork.OrderDetail.Add(orderDetail)));
        await _unitOfWork.Save();
    }

    private async Task<StatusCodeResult> CreatePaymentSession(ShoppingCartVM shoppingCartVM)
    {
        //var domain = "https://localhost:7014/";
        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        string currency = GetLocalCurrency();

        var lineItems = shoppingCartVM.ShoppingCartList.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                Currency = currency,
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.Product.Title
                }
            },
            Quantity = item.Count
        }).ToList();

        var options = new SessionCreateOptions
        {
            SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
            CancelUrl = domain + $"customer/cart/index",
            LineItems = lineItems,
            Mode = "payment",
        };

        var service = new SessionService();
        Session session = service.Create(options);

        _unitOfWork.OrderHeader.UpdateStripePaymentID(
            id: ShoppingCartVM.OrderHeader.Id,
            sessionId: session.Id,
            paymentIntentId: session.PaymentIntentId);

        await _unitOfWork.Save();
        Response.Headers.Append("Location", session.Url);

        return new StatusCodeResult(statusCode: (int)HttpStatusCode.RedirectMethod);
    }

    private async Task<OrderHeader?> GetOrderHeader(int orderId)
    {
        if (orderId == 0)
        {
            _logger.LogError(message: LogExceptionMessages.OrderIdNotFoundException);
            return null;
        }

        OrderHeader? orderHeader = await _unitOfWork.OrderHeader.Get(filter: u => u.Id == orderId, includeProperties: "ApplicationUser");

        if (orderHeader is null)
        {
            _logger.LogError(message: LogExceptionMessages.OrderNotFoundException);
            return null;
        }

        return orderHeader;
    }

    private async Task HandlePaymentStatus(OrderHeader orderHeader)
    {
        // order by customer
        var service = new SessionService();
        Session session = service.Get(id: orderHeader.SessionId);

        Dictionary<string, (string OrderStatus, string PaymentStatus)> paymentStatusUpdates = new()
            {
                { "paid", (SD.StatusApproved, SD.PaymentStatusApproved) },
                { "unpaid", (SD.StatusApproved, SD.PaymentStatusPending) },
                { "failed", (SD.StatusCancelled, SD.PaymentStatusRejected) }
            };

        string sessionPaymentStatus = session.PaymentStatus?.Trim() ?? string.Empty;

        if (paymentStatusUpdates.TryGetValue(sessionPaymentStatus, out var updateStatuses))
        {
            if (string.Equals(sessionPaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentID(
                    id: orderHeader.Id,
                    sessionId: session.Id,
                    paymentIntentId: session.PaymentIntentId
                );
            }

            _unitOfWork.OrderHeader.UpdateStatus(
                id: orderHeader.Id,
                orderStatus: SD.StatusApproved,
                paymentStatus: SD.PaymentStatusApproved
            );

            await _unitOfWork.Save();

            HttpContext.Session.Clear();
        }
    }

    private static string GetLocalCurrency()
    {
        var cultureInfo = CultureInfo.CurrentCulture;
        var regionInfo = new RegionInfo(cultureInfo.Name);
        return regionInfo.ISOCurrencySymbol.ToLower();
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

    private async Task SetSession(string userId)
    {
        var count = 0;

        IEnumerable<ShoppingCart>? shoppingCarts = await _unitOfWork.ShoppingCart.GetAll(filter: u => u.ApplicationUserId == userId);
        if (shoppingCarts is not null && shoppingCarts.Any())
            count = shoppingCarts.Count();

        HttpContext.Session.SetInt32(key: SD.SessionCart, value: count);
    }

    #endregion
}