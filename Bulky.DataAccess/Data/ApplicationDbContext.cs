using Bulky.DataAccess.Seeders;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        SetDecimalConvention(modelBuilder: modelBuilder);

        modelBuilder.ApplyConfiguration(configuration: new CategoriesSeeder());
        modelBuilder.ApplyConfiguration(configuration: new CompaniesSeeder());
        modelBuilder.ApplyConfiguration(configuration: new ProductsSeeder());
    }

    private static void SetDecimalConvention(ModelBuilder modelBuilder)
    {
        var decimalProperties = modelBuilder.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType.Equals(typeof(double)) || property.ClrType.Equals(typeof(double?)));

        foreach (var property in decimalProperties)
        {
            property.SetColumnType("decimal(38,2)");
        }
    }
}
