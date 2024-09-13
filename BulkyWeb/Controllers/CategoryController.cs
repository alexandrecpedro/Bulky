﻿using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CategoryController> _logger;
    public CategoryController(ApplicationDbContext db, ILogger<CategoryController> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        List<Category> objCategoryList = await _db.Categories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return View(objCategoryList);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
