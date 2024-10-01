using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulky.DataAccess.Seeders;

public class CompaniesSeeder : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasData(
            new Company
            {
                Id = 1,
                Name = "Tech Solution",
                StreetAddress = "123 Tech St",
                City = "Tech City",
                State = "IL",
                PostalCode = "12121",
                PhoneNumber = "6669990000"
            },
            new Company
            {
                Id = 2,
                Name = "Vivid Books",
                StreetAddress = "999 Vid St",
                City = "Vid City",
                State = "IL",
                PostalCode = "66666",
                PhoneNumber = "7779990000"
            },
            new Company
            {
                Id = 3,
                Name = "Readers Club",
                StreetAddress = "999 Main St",
                City = "Lala land",
                State = "NY",
                PostalCode = "99999",
                PhoneNumber = "1113335555"
            }
        );
    }
}
