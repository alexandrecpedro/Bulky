using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories;

[BindProperties]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CreateModel> _logger;

    public Category Category { get; set; }
    public CreateModel(ApplicationDbContext db, ILogger<CreateModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        _logger.LogInformation("Starting creating a category");

        await _db.Categories.AddAsync(Category);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
