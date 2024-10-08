using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Bulky.Utility.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Climate;
using System.Linq.Expressions;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = nameof(RoleEnum.Admin))]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderController> _logger;

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

    public async Task<IActionResult> Details(int orderId, int page = 1, int pageSize = 10)
    {
        _logger.LogInformation($"Starting showing order details...");

        if (orderId == 0)
        {
            _logger.LogError($"Invalid order ID {orderId}!");
            return BadRequest();
        }

        var orderHeaderTask = GetOrderHeaderByOrderId(orderId: orderId);
        var orderDetailListTask = GetOrderDetailListByOrderId(
                orderId: orderId,
                page: page,
                pageSize: pageSize
            );
        await Task.WhenAll(orderHeaderTask, orderDetailListTask);

        var orderHeader = await orderHeaderTask;
        var orderDetailList = await orderDetailListTask;

        if (orderHeader is null || orderDetailList is null) return NotFound();

        OrderVM orderVM = new()
        {
            OrderHeader = orderHeader,
            OrderDetailList = orderDetailList ?? []
        };

        return View(orderVM);
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
    #endregion
}
