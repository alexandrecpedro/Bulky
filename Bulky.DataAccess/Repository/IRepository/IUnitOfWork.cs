namespace Bulky.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }

    ICompanyRepository Company { get; }

    IProductRepository Product { get; }

    Task Save();
}
