using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories;

[BindProperties]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EditModel> _logger;

    public Category Category { get; set; }

    public EditModel(ApplicationDbContext db, ILogger<EditModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task OnGet(int? id)
    {
        _logger.LogInformation("Start searching a category by id");

        if (id.HasValue && id > 0)
        {
            Category? CategoryFromDb = await _db.Categories.FindAsync(id);
            if (CategoryFromDb != null)
            {
                Category = CategoryFromDb;
            }
        }
    }

    public async Task<IActionResult> OnPost()
    {
        _logger.LogInformation("Start updating a category");

        if (ModelState.IsValid)
        {
            _db.Categories.Update(Category);
            await _db.SaveChangesAsync();
            TempData["success"] = "Category updated successfully";
            return RedirectToPage("Index");
        }

        return Page();
    }
}
