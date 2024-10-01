﻿using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public IApplicationUserRepository ApplicationUser { get; private set; }

    public ICategoryRepository Category { get; private set; }

    public ICompanyRepository Company { get; private set; }

    public IProductRepository Product { get; private set; }

    public IShoppingCartRepository ShoppingCart { get; private set; }

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        ApplicationUser = new ApplicationUserRepository(_db);
        Category = new CategoryRepository(_db);
        Company = new CompanyRepository(_db);
        Product = new ProductRepository(_db);
        ShoppingCart = new ShoppingCartRepository(_db);
    }

    public async Task Save()
    {
        await _db.SaveChangesAsync();
    }
}
