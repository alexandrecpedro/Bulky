using Bulky.DataAccess.Data;
using Bulky.DataAccess.Extensions;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    internal DbSet<T> dbSet;
    private static readonly char[] separator = [','];

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        this.dbSet = _db.Set<T>();
        
        if (typeof(T).Equals(typeof(Product)))
        {
            this.dbSet.OfType<Product>().Include(u => u.Category).Include(u => u.CategoryId);
        }

        //_db.Categories == dbSet;
        //_db.Products.Include(u => u.Category).Include(u => u.CategoryId);
    }

    public async Task Add(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public async Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
    {
        IQueryable<T> query = dbSet;
        if (tracked)
        {
            query = query.AsNoTracking();
        }
        query = query.Where(filter);

        query = Repository<T>.GetFullEntity(query: query, includeProperties: includeProperties);
        
        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAll(int page = 1, int pageSize = 10, string? includeProperties = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 10);

        string primaryKeyName = typeof(T).GetPrimaryKeyName();
        IQueryable<T> query = dbSet;

        query = GetFullEntity(query: query, includeProperties: includeProperties);

        query = query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(entity => EF.Property<object>(entity, primaryKeyName));
        return await query.ToListAsync();
    }

    public void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entity)
    {
        dbSet.RemoveRange(entity);
    }

    private static IQueryable<T> GetFullEntity(IQueryable<T> query, string? includeProperties)
    {
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties
                .Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }

        return query;
    }
}
