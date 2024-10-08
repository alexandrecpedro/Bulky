using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting order search all...");

        IEnumerable<OrderHeader> objOrderHeaderList = await _unitOfWork.OrderHeader.GetAll(
            page: page, 
            pageSize: pageSize, 
            includeProperties: "ApplicationUser"
        );

        return Json(new { data = objOrderHeaderList });
    }

    #endregion
}
