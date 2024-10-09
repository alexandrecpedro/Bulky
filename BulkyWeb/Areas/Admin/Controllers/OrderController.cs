using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Bulky.Utility.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Text;

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

        var objOrderHeaderListFilter = new Dictionary<string, Expression<Func<OrderHeader, bool>>>
        {
            ["pending"] = u => u.PaymentStatus == SD.PaymentStatusDelayedPayment,
            ["inprocess"] = u => u.OrderStatus == SD.StatusInProcess,
            ["completed"] = u => u.OrderStatus == SD.StatusShipped,
            ["approved"] = u => u.OrderStatus == SD.StatusApproved
        };

        Expression<Func<OrderHeader, bool>> filter = u => true;

        if (objOrderHeaderListFilter.TryGetValue(status, out var filterExpression))
        {
            filter = filterExpression;
        }

        IEnumerable<OrderHeader> objOrderHeaderList = await _unitOfWork.OrderHeader.GetAll(
            filter: filter,
            page: page, 
            pageSize: pageSize, 
            includeProperties: "ApplicationUser"
        );

        return Json(new { data = objOrderHeaderList });
    }

    #endregion

    #region Private Methods
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

        if (!string.IsNullOrWhiteSpace(OrderVM.OrderHeader.Carrier))
        {
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
        }
        if (!string.IsNullOrWhiteSpace(OrderVM.OrderHeader.TrackingNumber))
        {
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
        }

        return orderHeader;
    }
    #endregion
}
