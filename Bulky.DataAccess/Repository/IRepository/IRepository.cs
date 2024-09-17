using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(int page = 1, int pageSize = 10);
    Task<T?> Get(Expression<Func<T, bool>> filter);
    Task Add(T entity);
    Task Remove(T entity);
    Task RemoveRange(IEnumerable<T> entity);
}
