using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
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
        //_db.Categories == dbSet;
        _db.Products.Include(u => u.Category).Include(u => u.CategoryId);
    }

    public async Task Add(T entity)
    {
        await dbSet.AddAsync(entity);
    }

    public async Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        query = query.Where(filter);
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties
                .Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAll(int page = 1, int pageSize = 10, string? includeProperties = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Max(pageSize, 10);

        IQueryable<T> query = dbSet;

        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties
                .Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }

        query = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
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
}
