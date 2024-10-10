using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = nameof(RoleEnum.Admin))]
public class CategoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryController> _logger;
    public CategoryController(IUnitOfWork unitOfWork, ILogger<CategoryController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation(message: "Starting category search all...");

        IEnumerable<Category> objCategoryList = await _unitOfWork.Category.GetAll(page: page, pageSize: pageSize);

        return View(objCategoryList);
    }

    public IActionResult Create()
    {
        _logger.LogInformation(message: "Starting category creation form...");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        _logger.LogInformation(message: "Starting category creation...");

        if (category.Name == category.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name!");
        }
        //if (category.Name is not null && category.Name.Equals("test", StringComparison.CurrentCultureIgnoreCase))
        //{
        //    ModelState.AddModelError("", "Test is an invalid value!");
        //}
        if (ModelState.IsValid)
        {
            await _unitOfWork.Category.Add(category);
            await _unitOfWork.Save();
            TempData["success"] = SuccessDataMessages.CategoryCreatedSuccess;
            return RedirectToAction(actionName: nameof(Index));
        }

        return View();
    }

    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation(message: "Starting category edit form...");

        if (id == null || id == 0)
        {
            _logger.LogError(message: LogExceptionMessages.CategoryIdNotFoundException);
            return NotFound();
        }
        Category? categoryFromDb = await _unitOfWork.Category.Get(u => u.Id == id);
        //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
        //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

        if (categoryFromDb == null)
        {
            _logger.LogError(message: LogExceptionMessages.CategoryNotFoundException);
            return NotFound();
        }
        return View(categoryFromDb);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Category category)
    {
        _logger.LogInformation(message: "Starting category update...");

        if (category.Name == category.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name!");
        }
        //if (category.Name is not null && category.Name.Equals("test", StringComparison.CurrentCultureIgnoreCase))
        //{
        //    ModelState.AddModelError("", "Test is an invalid value!");
        //}
        if (ModelState.IsValid)
        {
            _unitOfWork.Category.Update(category);
            await _unitOfWork.Save();
            TempData["success"] = SuccessDataMessages.CategoryUpdatedSuccess;
            return RedirectToAction(actionName: nameof(Index));
        }

        return View();
    }

    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation(message: "Starting category search by id to delete...");

        if (id == null || id == 0)
        {
            _logger.LogError(message: LogExceptionMessages.CategoryIdNotFoundException);
            return NotFound();
        }
        Category? categoryFromDb = await _unitOfWork.Category.Get(u => u.Id == id);
        //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
        //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

        if (categoryFromDb == null)
        {
            _logger.LogError(message: LogExceptionMessages.CategoryNotFoundException);
            return NotFound();
        }
        return View(categoryFromDb);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeletePOST(int? id)
    {
        _logger.LogInformation("Starting category delete...");

        Category? categoryFromDb = await _unitOfWork.Category.Get(u => u.Id == id);
        if (categoryFromDb == null)
        {
            _logger.LogError(message: LogExceptionMessages.CategoryNotFoundException);
            return NotFound();
        }

        _unitOfWork.Category.Remove(categoryFromDb);
        await _unitOfWork.Save();
        TempData["success"] = SuccessDataMessages.CategoryDeletedSuccess;
        return RedirectToAction(actionName: nameof(Index));
    }
}
