using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category>
{
    Task Update(Category category);
    Task Save();
}
