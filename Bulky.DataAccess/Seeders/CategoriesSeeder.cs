using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulky.DataAccess.Seeders;

public class CategoriesSeeder : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasData(
            new Category 
            { 
                Id = 1, 
                Name = "Action", 
                DisplayOrder = 1 
            },
            new Category 
            { 
                Id = 2, 
                Name = "SciFi", 
                DisplayOrder = 2 
            },
            new Category 
            { 
                Id = 3, 
                Name = "History", 
                DisplayOrder = 3 
            }
        );
    }
}
