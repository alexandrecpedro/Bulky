using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Printing;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(page: page, pageSize: pageSize, includeProperties: "Category").ToList();
        
        return View(objProductList);
    }

    public async Task<IActionResult> Upsert(int? id)
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

        // UPDATE
        if (id != null && id >= 0)
        {
            Product? productFromDb = await _unitOfWork.Product.Get(u => u.Id == id);
            //Product? productFromDb1 = _db.Products.FirstOrDefault(u=>u.Id==id);
            //Product? productFromDb2 = _db.Products.Where(u=>u.Id==id).FirstOrDefault();

            if (productFromDb == null)
            {
                return NotFound();
            }

            productVM.Product = productFromDb;
        }

        // CREATE
        return View(productVM);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string successMessage;

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if (!string.IsNullOrWhiteSpace(productVM.Product.ImageUrl))
                {
                    // delete the old image
                    var resizeOldImageUrlPath = productVM.Product.ImageUrl.TrimStart('\\');
                    var oldImagePath = Path.Combine(wwwRootPath, resizeOldImageUrlPath);

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                string fileStreamPath = Path.Combine(productPath, fileName);
                using (var fileStream = new FileStream(fileStreamPath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ImageUrl = @$"\images\product\{fileName}";
            }

            if (productVM.Product.Id == 0)
            {
                await _unitOfWork.Product.Add(productVM.Product);
                successMessage = "Product created successfully!";
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
                successMessage = "Product updated successfully!";
            }
            
            await _unitOfWork.Save();
            TempData["success"] = successMessage;
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

    #region API CALLS

    [HttpGet]
    public IActionResult GetAll(int page = 1, int pageSize = 10)
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(page: page, pageSize: pageSize, includeProperties: "Category").ToList();

        return Json(new { data = objProductList });
    }

    #endregion
}
