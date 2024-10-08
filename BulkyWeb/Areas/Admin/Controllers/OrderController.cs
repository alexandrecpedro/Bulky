using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Bulky.Utility.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = nameof(RoleEnum.Admin))]
public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyController> _logger;

    public OrderController(IUnitOfWork unitOfWork, ILogger<CompanyController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting order showing all...");

        return View();
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
}
