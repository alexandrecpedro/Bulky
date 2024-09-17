using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepo;
    public CategoryController(ICategoryRepository db)
    {
        _categoryRepo = db;
    }
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var objCategoryList = _categoryRepo.GetAll();

        return View(objCategoryList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
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
            await _categoryRepo.Add(category);
            await _categoryRepo.Save();
            TempData["success"] = "Category created successfully!";
            return RedirectToAction("Index");
        }

        return View();
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        Category? categoryFromDb = await _categoryRepo.Get(u => u.Id == id);
        //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
        //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

        if (categoryFromDb == null)
        {
            return NotFound();
        }
        return View(categoryFromDb);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Category category)
    {

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
            _categoryRepo.Update(category);
            await _categoryRepo.Save();
            TempData["success"] = "Category updated successfully!";
            return RedirectToAction("Index");
        }

        return View();
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        Category? categoryFromDb = await _categoryRepo.Get(u => u.Id == id);
        //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
        //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

        if (categoryFromDb == null)
        {
            return NotFound();
        }
        return View(categoryFromDb);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeletePOST(int? id)
    {
        Category? categoryFromDb = await _categoryRepo.Get(u => u.Id == id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        _categoryRepo.Remove(categoryFromDb);
        await _categoryRepo.Save();
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index");
    }
}
