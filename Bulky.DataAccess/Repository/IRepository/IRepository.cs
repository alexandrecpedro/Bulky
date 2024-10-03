using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, int page = 1, int pageSize = 10, string? includeProperties = null);
    Task<T?> Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
    Task Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entity);
}
