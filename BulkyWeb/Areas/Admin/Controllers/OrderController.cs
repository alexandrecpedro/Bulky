using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = nameof(RoleEnum.Admin))]
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
            _logger.LogError($"Invalid order ID {orderId}!");
            return BadRequest();
        }

        var orderHeader = await GetOrderHeaderByOrderId(orderId: orderId);
        if (orderHeader is null) return NotFound();

        var orderDetailList = await GetOrderDetailListByOrderId(
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

        var orderHeaderFromDb = await GetOrderHeaderByOrderId(orderId: OrderVM.OrderHeader.Id);
        if (orderHeaderFromDb is null)
        {
            _logger.LogError($"Order header not found!");
            return BadRequest();
        }

        orderHeaderFromDb = MapProperties(orderHeader: orderHeaderFromDb);

        _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
        await _unitOfWork.Save();

        TempData["Success"] = "Order details updated successfully!";

        return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
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

    #region Private Methods

    private string? GetLoggedUserId()
    {
        // Get logged user
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("User ID not found!");
            return null;
        }

        return userId;
    }

    private async Task<OrderHeader?> GetOrderHeaderByOrderId(int orderId)
    {
        var orderHeader = await _unitOfWork.OrderHeader.Get(
            filter: u => u.Id == orderId,
            includeProperties: "ApplicationUser"
        );

        if (orderHeader is null)
        {
            _logger.LogError("Order header not found!");
            return null;
        }

        return orderHeader;
    }

    private async Task<IEnumerable<OrderDetail>?> GetOrderDetailListByOrderId(int orderId, int page = 1, int pageSize = 10)
    {
        var orderDetailList = await _unitOfWork.OrderDetail.GetAll(
            filter: u => u.OrderHeaderId == orderId,
            page: page,
            pageSize: pageSize,
            includeProperties: "Product"
        );

        if (orderDetailList is null || !orderDetailList.Any())
        {
            _logger.LogError("Order detail list not found!");
            return null;
        }

        return orderDetailList;
    }

    private OrderHeader MapProperties(OrderHeader orderHeader)
    {
        orderHeader.Name = OrderVM.OrderHeader.Name;
        orderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
        orderHeader.StreetAddress = OrderVM.OrderHeader.StreetAddress;
        orderHeader.City = OrderVM.OrderHeader.City;
        orderHeader.State = OrderVM.OrderHeader.State;
        orderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;

        orderHeader.Carrier = !string.IsNullOrWhiteSpace(OrderVM.OrderHeader.Carrier)
            ? OrderVM.OrderHeader.Carrier
            : orderHeader.Carrier;

        orderHeader.Carrier = !string.IsNullOrWhiteSpace(OrderVM.OrderHeader.TrackingNumber)
            ? OrderVM.OrderHeader.TrackingNumber
            : orderHeader.TrackingNumber;

        return orderHeader;
    }

    private async Task<IEnumerable<OrderHeader>?> GetFilteredOrderHeaders(string status, int page, int pageSize)
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

        if (orderHeaderList is null)
        {
            _logger.LogError("Order header list not found!");
            return null;
        }

        return orderHeaderList;
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
            _logger.LogError("User ID not found!");
            return u => false;
        }
        return u => u.ApplicationUserId == userId;
    }

    private Expression<Func<OrderHeader, bool>> GetStatusFilter(string status)
    {
        var objOrderHeaderListFilter = new Dictionary<string, Expression<Func<OrderHeader, bool>>>
        {
            ["pending"] = (u => u.PaymentStatus == SD.PaymentStatusDelayedPayment),
            ["inprocess"] = (u => u.OrderStatus == SD.StatusInProcess),
            ["completed"] = (u => u.OrderStatus == SD.StatusShipped),
            ["approved"] = (u => u.OrderStatus == SD.StatusApproved)
        };

        return objOrderHeaderListFilter.TryGetValue(status, out var filterExpression)
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

    #endregion
}
