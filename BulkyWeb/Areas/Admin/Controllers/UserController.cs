using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility.Enum;
using Bulky.Utility.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = nameof(RoleEnum.Admin))]
public class UserController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserController> _logger;

    public UserController(IUnitOfWork unitOfWork, ILogger<UserController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting user display all...");

        return View();
    }

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting user search all...");

        IEnumerable<ApplicationUser> objUserList = await _unitOfWork.ApplicationUser.GetAll(
            page: page, 
            pageSize: pageSize,
            includeProperties: "Company"
        );

        await Task.WhenAll(objUserList.Select(async user =>
        {
            //user.Role = ;

            user.Company ??= new Company
                {
                    Name = ""
                };

            return user;
        }));

        return Json(new { data = objUserList });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string? id)
    {
        _logger.LogInformation("Starting user delete...");

        var applicationUserToBeDeleted = await _unitOfWork.ApplicationUser.Get(u => u.Id == id);

        if (applicationUserToBeDeleted is null)
        {
            return Json(new { success = false, message = LogExceptionMessages.CompanyDeleteException });
        }

        _unitOfWork.ApplicationUser.Remove(applicationUserToBeDeleted);
        await _unitOfWork.Save();

        return Json(new { success = true, message = SuccessDataMessages.UserDeletedSuccess });
    }

    #endregion
}
