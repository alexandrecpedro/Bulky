using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(page: page, pageSize: pageSize).ToList();
        
        return View(objProductList);
    }

    public IActionResult Create()
    {
        ProductVM productVM = new()
        {
            CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
            Product = new Product()
        };
        //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
        //    .GetAll().Select(u => new SelectListItem
        //    {
        //        Text = u.Name,
        //        Value = u.Id.ToString(),
        //    });

        //ViewBag.CategoryList = CategoryList;
        //ViewData["CategoryList"] = CategoryList;

        return View(productVM);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductVM productVM)
    {
        if (ModelState.IsValid)
        {
            await _unitOfWork.Product.Add(productVM.Product);
            await _unitOfWork.Save();
            TempData["success"] = "Product created successfully!";
            return RedirectToAction("Index");
        }

        productVM.CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

        return View(productVM);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        Product? productFromDb = await _unitOfWork.Product.Get(u => u.Id == id);
        //Product? productFromDb1 = _db.Products.FirstOrDefault(u=>u.Id==id);
        //Product? productFromDb2 = _db.Products.Where(u=>u.Id==id).FirstOrDefault();
        
        if (productFromDb == null)
        {
            return NotFound();
        }
        return View(productFromDb);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Product.Update(product);
            await _unitOfWork.Save();
            TempData["success"] = "Product updated successfully!";
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
        Product? productFromDb = await _unitOfWork.Product.Get(u => u.Id == id);
        //Product? productFromDb1 = _db.Products.FirstOrDefault(u=>u.Id==id);
        //Product? productFromDb2 = _db.Products.Where(u=>u.Id==id).FirstOrDefault();

        if (productFromDb == null)
        {
            return NotFound();
        }
        return View(productFromDb);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeletePOST(int? id)
    {
        Product? productFromDb = await _unitOfWork.Product.Get(u => u.Id == id);
        if (productFromDb == null)
        {
            return NotFound();
        }

        _unitOfWork.Product.Remove(productFromDb);
        await _unitOfWork.Save();
        TempData["success"] = "Product deleted successfully!";
        return RedirectToAction("Index");
    }
}
