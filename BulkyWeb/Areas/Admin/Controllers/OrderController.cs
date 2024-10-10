using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderController> _logger;
    [BindProperty]
    public OrderVM OrderVM { get; set; }

    public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting order showing all...");

        return View();
    }

    public async Task<IActionResult> Details(int orderId, int? page, int? pageSize)
    {
        _logger.LogInformation($"Starting showing order details...");

        if (orderId == 0)
        {
            _logger.LogError(message: LogExceptionMessages.OrderHeaderIdInvalidDataException);
            return BadRequest();
        }

        var orderHeader = await GetOrderHeaderById(orderId: orderId);
        if (orderHeader is null) return NotFound();

        var orderDetailList = await GetOrderDetailListByOrderHeaderId(
                orderId: orderId,
                page: page ?? 1,
                pageSize: pageSize ?? 10
            );
        if (orderDetailList is null) return NotFound();

        OrderVM = new()
        {
            OrderHeader = orderHeader,
            OrderDetailList = orderDetailList ?? []
        };

        return View(OrderVM);
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin_Employee)]
    public async Task<IActionResult> UpdateOrderDetail()
    {
        _logger.LogInformation($"Starting update order details...");

        var orderHeaderFromDb = await GetOrderHeaderById(orderId: OrderVM.OrderHeader.Id);
        if (orderHeaderFromDb is null) return NotFound();

        orderHeaderFromDb = MapProperties(orderHeader: orderHeaderFromDb, orderStep: nameof(UpdateOrderDetail));

        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        await _unitOfWork.Save();

        TempData["Success"] = SuccessDataMessages.OrderDetailUpdatedSuccess;

        return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin_Employee)]
    public async Task<IActionResult> StartProcessing()
    {
        _logger.LogInformation("Starting order processing...");

        _unitOfWork.OrderHeader.UpdateStatus(
            id: OrderVM.OrderHeader.Id,
            orderStatus: SD.StatusInProcess
        );
        await _unitOfWork.Save();

        TempData["Success"] = SuccessDataMessages.OrderDetailUpdatedSuccess;
        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin_Employee)]
    public async Task<IActionResult> ShipOrder()
    {
        OrderHeader? orderHeader = await GetOrderHeaderById(orderId: OrderVM.OrderHeader.Id);
        if (orderHeader is null) return NotFound();

        orderHeader = MapProperties(orderHeader: orderHeader, orderStep: nameof(ShipOrder));

        _unitOfWork.OrderHeader.Update(orderHeader);
        await _unitOfWork.Save();

        TempData["Success"] = SuccessDataMessages.OrderShippedSuccess;
        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [Authorize(Roles = SD.Role_Admin_Employee)]
    public async Task<IActionResult> CancelOrder()
    {
        OrderHeader? orderHeader = await GetOrderHeaderById(orderId: OrderVM.OrderHeader.Id);
        if (orderHeader is null) return NotFound();

        var paymentStatus = (orderHeader.PaymentStatus != SD.PaymentStatusApproved) 
            ? SD.StatusRefunded
            : SD.StatusCancelled;

        var options = new RefundCreateOptions
        {
            Reason = RefundReasons.RequestedByCustomer,
            PaymentIntent = orderHeader.PaymentIntentId
        };

        var service = new RefundService();
        Refund refund = service.Create(options: options);

        _unitOfWork.OrderHeader.UpdateStatus(
            id: orderHeader.Id,
            orderStatus: SD.StatusCancelled,
            paymentStatus: paymentStatus
        );
        await _unitOfWork.Save();

        TempData["success"] = SuccessDataMessages.OrderCancelledSuccess;
        return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
    }

    [HttpPost]
    [ActionName("Details")]
    public async Task<IActionResult> Details_PAY_NOW(int? page, int? pageSize)
    {
        var orderHeader = await GetOrderHeaderById(orderId: OrderVM.OrderHeader.Id);
        if (orderHeader is null) return NotFound();
        OrderVM.OrderHeader = orderHeader;

        var orderDetailList = await GetOrderDetailListByOrderHeaderId(
                orderId: OrderVM.OrderHeader.Id,
                page: page ?? 1,
                pageSize: pageSize ?? 10
        );
        if (orderDetailList is null) return NotFound();
        OrderVM.OrderDetailList = orderDetailList;

        //stripe logic
        return await CreatePaymentSession();
    }

    public async Task<IActionResult> PaymentConfirmation(int orderHeaderId)
    {
        OrderHeader? orderHeader = await GetOrderHeaderById(orderId: orderHeaderId);
        if (orderHeader is null) return NotFound();

        if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            // company's order
            var service = new SessionService();
            Session session = service.Get(id: orderHeader.SessionId);

            if (String.Equals(session.PaymentStatus, "paid", StringComparison.CurrentCultureIgnoreCase))
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentID(
                    id: orderHeaderId,
                    sessionId: session.Id,
                    paymentIntentId: session.PaymentIntentId
                );

                _unitOfWork.OrderHeader.UpdateStatus(
                    id: orderHeaderId,
                    orderStatus: orderHeader.OrderStatus ?? SD.StatusShipped,
                    paymentStatus: SD.PaymentStatusApproved
                );

                await _unitOfWork.Save();
            }
        }

        return View(orderHeaderId);
    }

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll(string status, int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting order search all...");

        IEnumerable<OrderHeader> objOrderHeaderList = await GetFilteredOrderHeaders(
            status: status,
            page: page,
            pageSize: pageSize
        );

        return Json(new { data = objOrderHeaderList });
    }

    #endregion

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

    private async Task<OrderHeader?> GetOrderHeaderById(int orderId)
    {
        var orderHeader = await _unitOfWork.OrderHeader.Get(
            filter: u => u.Id == orderId,
            includeProperties: "ApplicationUser"
        );

        if (orderHeader is null)
        {
            _logger.LogError(message: LogExceptionMessages.OrderHeaderNotFoundException);
            return null;
        }

        return orderHeader;
    }

    private async Task<IEnumerable<OrderDetail>?> GetOrderDetailListByOrderHeaderId(int orderId, int page = 1, int pageSize = 10)
    {
        var orderDetailList = await _unitOfWork.OrderDetail.GetAll(
            filter: u => u.OrderHeaderId == orderId,
            page: page,
            pageSize: pageSize,
            includeProperties: "Product"
        );

        if (orderDetailList is null || !orderDetailList.Any())
        {
            _logger.LogError(message: LogExceptionMessages.OrderDetailListNotFoundException);
            return null;
        }

        return orderDetailList;
    }

    private OrderHeader MapProperties(OrderHeader orderHeader, string orderStep)
    {
        MapProperty(orderHeader, o => o.Name, OrderVM.OrderHeader.Name);
        MapProperty(orderHeader, o => o.PhoneNumber, OrderVM.OrderHeader.PhoneNumber);
        MapProperty(orderHeader, o => o.StreetAddress, OrderVM.OrderHeader.StreetAddress);
        MapProperty(orderHeader, o => o.City, OrderVM.OrderHeader.City);
        MapProperty(orderHeader, o => o.State, OrderVM.OrderHeader.State);
        MapProperty(orderHeader, o => o.PostalCode, OrderVM.OrderHeader.PostalCode);
        MapProperty(orderHeader, o => o.Carrier, OrderVM.OrderHeader.Carrier);
        MapProperty(orderHeader, o => o.TrackingNumber, OrderVM.OrderHeader.TrackingNumber);

        if (orderStep.Trim() == nameof(ShipOrder))
        {
            MapProperty(orderHeader, o => o.OrderStatus, SD.StatusShipped);
            MapProperty(orderHeader, o => o.ShippingDate, DateTime.UtcNow);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
            }
        }

        return orderHeader;
    }

    private void MapProperty<T>(OrderHeader target, Expression<Func<OrderHeader, T>> targetPropertyExpression, T newValue)
    {
        if (newValue == null) return;

        var memberExpression = (MemberExpression)targetPropertyExpression.Body;
        var propertyInfo = (PropertyInfo)memberExpression.Member;

        var currentValue = propertyInfo.GetValue(target);

        if (currentValue is null || !EqualityComparer<T>.Default.Equals((T)currentValue, newValue))
        {
            propertyInfo.SetValue(target, newValue);
        }
    }

    private object? GetTargetObject(MemberExpression memberExpression)
    {
        return memberExpression.Expression switch
        {
            ConstantExpression constantExpression => constantExpression.Value,
            MemberExpression innerMemberExpression => Expression.Lambda(innerMemberExpression).Compile().DynamicInvoke(),
            _ => null
        };
    }

    private async Task<IEnumerable<OrderHeader>> GetFilteredOrderHeaders(string status, int page, int pageSize)
    {
        Expression<Func<OrderHeader, bool>> orderFilter = GetOrderFilter(status: status);

        Expression<Func<OrderHeader, bool>> statusFilter = GetStatusFilter(status: status);

        Expression<Func<OrderHeader, bool>> combinedFilter = CombineFilters(orderFilter: orderFilter, statusFilter: statusFilter);

        var orderHeaderList = await _unitOfWork.OrderHeader.GetAll(
            filter: combinedFilter,
            page: page,
            pageSize: pageSize,
            includeProperties: "ApplicationUser"
        );

        return orderHeaderList ?? [];
    }

    private Expression<Func<OrderHeader, bool>> GetOrderFilter(string status)
    {
        var isAdmin = User.IsInRole(SD.Role_Admin);
        var isEmployee = User.IsInRole(SD.Role_Employee);

        if (isAdmin || isEmployee)
            return u => true;

        var userId = GetLoggedUserId();
        return CreateUserFilter(userId: userId);
    }

    private Expression<Func<OrderHeader, bool>> CreateUserFilter(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError(message: LogExceptionMessages.UserIdNotFoundException);
            return u => false;
        }
        return u => u.ApplicationUserId == userId;
    }

    private Expression<Func<OrderHeader, bool>> GetStatusFilter(string status)
    {
        var filters = new Dictionary<string, Expression<Func<OrderHeader, bool>>>
        {
            ["pending"] = (u => u.PaymentStatus == SD.PaymentStatusDelayedPayment),
            ["inprocess"] = (u => u.OrderStatus == SD.StatusInProcess),
            ["completed"] = (u => u.OrderStatus == SD.StatusShipped),
            ["approved"] = (u => u.OrderStatus == SD.StatusApproved)
        };

        return filters.TryGetValue(status, out var filterExpression)
            ? filterExpression
            : u => true;
    }

    private Expression<Func<OrderHeader, bool>> CombineFilters(
        Expression<Func<OrderHeader, bool>> orderFilter,
        Expression<Func<OrderHeader, bool>> statusFilter
    )
    {
        var parameter = Expression.Parameter(typeof(OrderHeader), "u");

        var combined = Expression.AndAlso(
            Expression.Invoke(orderFilter, parameter),
            Expression.Invoke(statusFilter, parameter)
        );

        return Expression.Lambda<Func<OrderHeader, bool>>(combined, parameter);
    }

    private async Task<StatusCodeResult> CreatePaymentSession()
    {
        //var domain = "https://localhost:7014/";
        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        string currency = GetLocalCurrency();

        var lineItems = OrderVM.OrderDetailList.Select(item => new SessionLineItemOptions
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
            SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
            CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
            LineItems = lineItems,
            Mode = "payment",
        };

        var service = new SessionService();
        Session session = service.Create(options: options);

        _unitOfWork.OrderHeader.UpdateStripePaymentID(
            id: OrderVM.OrderHeader.Id,
            sessionId: session.Id,
            paymentIntentId: session.PaymentIntentId
        );

        await _unitOfWork.Save();
        Response.Headers.Append("Location", session.Url);

        return new StatusCodeResult(statusCode: (int)HttpStatusCode.RedirectMethod);
    }

    private static string GetLocalCurrency()
    {
        var cultureInfo = CultureInfo.CurrentCulture;
        var regionInfo = new RegionInfo(cultureInfo.Name);
        return regionInfo.ISOCurrencySymbol.ToLower();
    }

    #endregion
}
