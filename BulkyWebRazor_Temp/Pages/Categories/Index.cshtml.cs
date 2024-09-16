using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_Temp.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<IndexModel> _logger;
    public List<Category> CategoryList { get; set; } 
    public IndexModel(ApplicationDbContext db, ILogger<IndexModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task OnGet(int page = 1, int pageSize = 10)
    {
        _logger.LogInformation("Start searching all categories");
        CategoryList = await _db.Categories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }
}
