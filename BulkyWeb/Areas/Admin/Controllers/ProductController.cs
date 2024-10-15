using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility.Enum;
using Bulky.Utility.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = nameof(RoleEnum.Admin))]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ProductController> _logger;
    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<ProductController> logger)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting product search all including category...");

        IEnumerable<Product> objProductList = await _unitOfWork.Product.GetAll(page: page, pageSize: pageSize, includeProperties: "Category");
        
        return View(objProductList);
    }

    public async Task<IActionResult> Upsert(int? id)
    {
        _logger.LogInformation("Starting product upsert form...");

        ProductVM? productVM = await CreateProductViewModel(id: id);
        if(productVM is null)
            return NotFound();

        return View(productVM);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert(ProductVM productVM, List<IFormFile>? files)
    {
        _logger.LogInformation("Starting product upsert...");

        if (!ModelState.IsValid)
        {
            var newProductVM = await PopulateCategoryListAsync(productVM: productVM);
            return View(newProductVM);
        }

        string successMessage = productVM.Product.Id == 0
            ? SuccessDataMessages.ProductCreatedSuccess
            : SuccessDataMessages.ProductUpdatedSuccess;
            

        if (productVM.Product.Id == 0)
        {
            await _unitOfWork.Product.Add(productVM.Product);
        }
        else
        {
            _unitOfWork.Product.Update(productVM.Product);
        }
            
        await _unitOfWork.Save();

        if (files?.Any() == true)
        {
            var product = await ProcessUploadedFilesAsync(files: files, product: productVM.Product);
            productVM.Product = product;
            _unitOfWork.Product.Update(productVM.Product);
            await _unitOfWork.Save();
        }

        TempData["success"] = successMessage;
        return RedirectToAction(actionName: nameof(Index));
    }

    #region API CALLS

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Starting product search all...");

        IEnumerable<Product> objProductList = await _unitOfWork.Product.GetAll(page: page, pageSize: pageSize, includeProperties: "Category");

        return Json(new { data = objProductList });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation("Starting product delete...");

        var productToBeDeleted = await _unitOfWork.Product.Get(u => u.Id == id);

        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = LogExceptionMessages.ProductNotFoundException });
        }

        // remove old image
        string wwwRootPath = _webHostEnvironment.WebRootPath;
        //var resizeOldImageUrlPath = productToBeDeleted.ImageUrl.TrimStart('\\');
        //var oldImagePath = Path.Combine(wwwRootPath, resizeOldImageUrlPath);

        //if (System.IO.File.Exists(oldImagePath))
        //{
        //    System.IO.File.Delete(oldImagePath);
        //}

        _unitOfWork.Product.Remove(productToBeDeleted);
        await _unitOfWork.Save();

        return Json(new { success = true, message = SuccessDataMessages.ProductDeletedSuccess });
    }

    #endregion

    #region PRIVATE METHODS
    private async Task<Product> ProcessUploadedFilesAsync(List<IFormFile> files, Product product)
    {
        string wwwRootPath = _webHostEnvironment.WebRootPath;
        string productPath = Path.Combine("images", "products", "product-" + product.Id);

        foreach (IFormFile file in files)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string finalPath = Path.Combine(wwwRootPath, productPath);

            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            string fileStreamPath = Path.Combine(finalPath, fileName);
            using (var fileStream = new FileStream(path: fileStreamPath, mode: FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            ProductImage productImage = new()
            {
                ImageUrl = @"\" + productPath + @"\" + fileName,
                ProductId = product.Id,
            };

            product.ProductImages ??= new List<ProductImage>();
            product.ProductImages.Add(productImage);
        }
        
        return product;
    }

    private async Task<ProductVM> PopulateCategoryListAsync(ProductVM productVM)
    {
        var categories = await _unitOfWork.Category.GetAll();
        productVM.CategoryList = categories.Select(u => new SelectListItem
        {
            Text = u.Name,
            Value = u.Id.ToString()
        });

        return productVM;
    }

    private async Task<ProductVM?> CreateProductViewModel(int? id)
    {
        var categoryListAsync = await _unitOfWork.Category.GetAll();
        var productVM = new ProductVM
        {
            CategoryList = categoryListAsync.Select(u => new SelectListItem
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
                _logger.LogError(message: LogExceptionMessages.ProductNotFoundException);
                return null;
            }

            productVM.Product = productFromDb;
        }

        return productVM;
    }
    #endregion
}
