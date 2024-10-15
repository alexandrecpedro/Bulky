using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Bulky.Utility.Enum;
using Bulky.Utility.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = nameof(RoleEnum.Admin))]
public class UserController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserController> _logger;

    public UserController(
        UserManager<IdentityUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IUnitOfWork unitOfWork, 
        ILogger<UserController> logger
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Starting user display all...");

        return View();
    }
    
    public async Task<IActionResult> RoleManagement(string userId)
    {
        if (userId.Trim() is null)
        {
            _logger.LogError(message: LogExceptionMessages.UserIdNotFoundException);
            return BadRequest();
        }

        var applicationUser = await _unitOfWork.ApplicationUser.Get(
            filter: u => u.Id == userId,
            includeProperties: "Company"
        );
        if (applicationUser is null)
        {
            _logger.LogError(message: LogExceptionMessages.UserNotFoundException);
            return NotFound();
        }

        var roleList = _roleManager.Roles.Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Name
        });

        var companyList = _unitOfWork.Company.GetAll().GetAwaiter().GetResult().Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Id.ToString()
        });

        RoleManagementVM RoleVM = new()
        {
            ApplicationUser = applicationUser,
            RoleList = roleList,
            CompanyList = companyList
        };

        RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(user: applicationUser).GetAwaiter().GetResult().FirstOrDefault() ?? SD.Role_Customer;

        return View(RoleVM);
    }

    [HttpPost]
    public async Task<IActionResult> RoleManagement(RoleManagementVM roleManagementVM)
    {
        var applicationUser = await _unitOfWork.ApplicationUser.Get(
            filter: u => u.Id == roleManagementVM.ApplicationUser.Id
        );
        if (applicationUser is null)
        {
            _logger.LogError(message: LogExceptionMessages.UserNotFoundException);
            return NotFound();
        }

        string oldRole = _userManager.GetRolesAsync(user: applicationUser)
                                .GetAwaiter()
                                .GetResult()
                                .FirstOrDefault() ?? SD.Role_Customer;

        var newRole = roleManagementVM.ApplicationUser.Role;

        if (newRole != oldRole)
        {
            await UpdateUserRoleAsync(
                applicationUser: applicationUser,
                oldRole: oldRole,
                newRole: newRole,
                roleManagementVM: roleManagementVM
            );
        }
        else
        {
            await UpdateCompanyIdIfNeeded(
                applicationUser: applicationUser,
                roleManagementVM: roleManagementVM,
                oldRole: oldRole
            );
        }

        return RedirectToAction(actionName: nameof(Index));
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

        var updatedUserList = objUserList.Select(user =>
        {
            user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault() ?? SD.Role_Customer;

            user.Company ??= new Company { Name = "" };

            return user;
        }).ToList();

        return Json(new { data = updatedUserList });
    }

    [HttpPost]
    public async Task<IActionResult> LockUnlock([FromBody] string id)
    {
        var userFromDb = await _unitOfWork.ApplicationUser.Get(filter: u => u.Id == id);
        if (userFromDb is null)
        {
            return Json(new { success = false, message = LogExceptionMessages.LockUnlockException });
        }

        if (userFromDb.LockoutEnd is not null && userFromDb.LockoutEnd > DateTime.UtcNow)
        {
            // user is current locked and we need to unlock them
            userFromDb.LockoutEnd = DateTime.UtcNow;
        }
        else
        {
            userFromDb.LockoutEnd = DateTime.UtcNow.AddYears(1000);
        }

        _unitOfWork.ApplicationUser.Update(applicationUser: userFromDb);
        await _unitOfWork.Save();
        return Json(new { success = true, message = SuccessDataMessages.UserUpdatedSuccess });
    }
    #endregion

    #region PRIVATE METHODS
    private async Task UpdateUserRoleAsync(
        ApplicationUser applicationUser,
        string oldRole,
        string newRole,
        RoleManagementVM roleManagementVM
    )
    {
        UpdateCompanyId(
            applicationUser: applicationUser,
            oldRole: oldRole,
            newRole: newRole,
            companyId: roleManagementVM.ApplicationUser.CompanyId
        );

        _unitOfWork.ApplicationUser.Update(applicationUser);
        await _unitOfWork.Save();

        await _userManager.RemoveFromRoleAsync(user: applicationUser, role: oldRole);
        await _userManager.AddToRoleAsync(user: applicationUser, role: newRole);
    }

    private async Task UpdateCompanyIdIfNeeded(
        ApplicationUser applicationUser, 
        RoleManagementVM roleManagementVM, 
        string oldRole
    )
    {
        var isOldRoleCompany = oldRole == SD.Role_Company;
        var isNewUserCompanyId = applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId;

        if (isOldRoleCompany && isNewUserCompanyId)
        {
            applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
            _unitOfWork.ApplicationUser.Update(applicationUser);
            await _unitOfWork.Save();
        }
    }

    private void UpdateCompanyId(
        ApplicationUser applicationUser, 
        string oldRole, 
        string newRole, 
        int? companyId
    )
    {
        var isOldRoleCompany = oldRole == SD.Role_Company;
        var isNewRoleCompany = newRole == SD.Role_Company;

        if (!isNewRoleCompany && isOldRoleCompany)
        {
            applicationUser.CompanyId = null;
            return;
        }

        applicationUser.CompanyId = companyId;
    }
    #endregion
}
