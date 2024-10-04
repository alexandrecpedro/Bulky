namespace Bulky.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    IApplicationUserRepository ApplicationUser { get; }

    ICategoryRepository Category { get; }

    ICompanyRepository Company { get; }

    IOrderDetailRepository OrderDetail { get; }

    IOrderHeaderRepository OrderHeader { get; }

    IProductRepository Product { get; }

    IShoppingCartRepository ShoppingCart { get; }

    Task Save();
}
