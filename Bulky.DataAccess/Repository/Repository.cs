﻿using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        this.dbSet = _db.Set<T>();
        //_db.Categories == dbSet;
    }

    public async Task Add(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public async Task<T?> Get(Expression<Func<T, bool>> filter)
    {
        IQueryable<T> query = dbSet.AsNoTracking();
        query = query.Where(filter);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAll(int page = 1, int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 10);

        IEnumerable<T> query = dbSet.AsNoTracking();
        query = query.Skip((page - 1) * pageSize)
            .Take(pageSize);
        return await query.AsQueryable().ToListAsync();
    }

    public async Task Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public async Task RemoveRange(IEnumerable<T> entity)
    {
        dbSet.RemoveRange(entity);
    }
}
