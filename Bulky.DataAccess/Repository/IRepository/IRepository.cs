using System.Linq.Expressions;

namespace Bulky.DataAccess.Repository.IRepository;

internal interface IRepository<T> where T : class
{
    IQueryable<T> GetAll(int? page, int? pageSize);
    T Get(Expression<Func<T, bool>> filter);
    void Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entity);
}
