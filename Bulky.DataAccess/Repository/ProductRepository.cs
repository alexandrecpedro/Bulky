using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Product product)
    {
        var productFromDb = _db.Products.FirstOrDefault(u => u.Id == product.Id);
        if (productFromDb != null)
        {
            productFromDb.Title = product.Title;
            productFromDb.Author = product.Author;
            productFromDb.ISBN = product.ISBN;
            productFromDb.ListPrice = product.ListPrice;
            productFromDb.Price = product.Price;
            productFromDb.Price50 = product.Price50;
            productFromDb.Price100 = product.Price100;
            productFromDb.Description = product.Description;
            product.CategoryId = product.CategoryId;
        }
    }
}
