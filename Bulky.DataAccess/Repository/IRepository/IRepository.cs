using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll(int page = 1, int pageSize = 10);
    Task<T?> Get(Expression<Func<T, bool>> filter);
    Task Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entity);
}
