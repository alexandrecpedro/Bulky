using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = nameof(RoleEnum.Admin))]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(IUnitOfWork unitOfWork, ILogger<CompanyController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting company search all...");
        IEnumerable<Company> objCompanyList = await _unitOfWork.Company.GetAll(page: page, pageSize: pageSize);

        return View(objCompanyList);
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        _logger.LogInformation("Starting company upsert form...");

        Company companyObj = new();

        if (id != null && id >= 0)
        {
            Company? companyFromDb = await _unitOfWork.Company.Get(u => u.Id == id);

            if (companyFromDb is null)
            {
                _logger.LogError(message: LogExceptionMessages.CompanyNotFoundException);
                return NotFound();
            }

            companyObj = companyFromDb;
        }

        return View(companyObj);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(Company company)
    {
        _logger.LogInformation("Starting company upsert...");

        if (ModelState.IsValid)
        {
            string successMessage;

            if (company.Id is 0)
            {
                await _unitOfWork.Company.Add(company);
                successMessage = SuccessDataMessages.CompanyCreatedSuccess;
            }
            else
            {
                _unitOfWork.Company.Update(company);
                successMessage = SuccessDataMessages.CompanyUpdatedSuccess;
            }

            await _unitOfWork.Save();
            TempData["success"] = successMessage;
            return RedirectToAction(actionName: nameof(Index));
        }

        return View(company);
    }

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting company search all...");

        IEnumerable<Company> objCompanyList = await _unitOfWork.Company.GetAll(page: page, pageSize: pageSize);

        return Json(new { data = objCompanyList });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation("Starting company delete...");

        var companyToBeDeleted = await _unitOfWork.Company.Get(u => u.Id == id);

        if (companyToBeDeleted is null)
        {
            return Json(new { success = false, message = LogExceptionMessages.CompanyDeleteException });
        }

        _unitOfWork.Company.Remove(companyToBeDeleted);
        await _unitOfWork.Save();

        return Json(new { success = true, message = SuccessDataMessages.CategoryDeletedSuccess });
    }

    #endregion
}
