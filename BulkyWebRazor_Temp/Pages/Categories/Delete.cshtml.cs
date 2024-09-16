using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories;

[BindProperties]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DeleteModel> _logger;
    public Category Category { get; set; }

    public DeleteModel(ApplicationDbContext db, ILogger<DeleteModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task OnGet(int? id)
    {
        _logger.LogInformation("Start searching a category by id");

        if (id.HasValue && id > 0)
        {
            Category? categoryFromDb = await _db.Categories.FindAsync(id);
            if (categoryFromDb != null)
            {
                Category = categoryFromDb;
            }
        }
    }

    public async Task<IActionResult> OnPost()
    {
        _logger.LogInformation("Start deleting a category");

        Category? categoryFromDb = await _db.Categories.FindAsync(Category.Id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        _db.Categories.Remove(categoryFromDb);
        await _db.SaveChangesAsync();
        TempData["success"] = "Category deleted successfully";
        return RedirectToPage("Index");
    }
}
