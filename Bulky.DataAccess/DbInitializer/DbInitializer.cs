using Bulky.DataAccess.Data;
using Bulky.DataAccess.DbInitializer.Interfaces;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.DbInitializer;

public class DbInitializer : IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public DbInitializer(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public void Initialize()
    {
        //migrations if they are not applied
        try
        {
            if (_db.Database.GetPendingMigrations().Any())
            {
                _db.Database.Migrate();
            }
        } catch (Exception) { }

        //create roles if they are not created
        var roleCustomExists = _roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult();

        if (!roleCustomExists)
        {
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

            //if roles are not created, then we will create admin user as well
            ApplicationUser adminUser = new()
            {
                UserName = "testadmin@gmail.com",
                Email = "testadmin@gmail.com",
                Name = "Ben Peter (Admin)",
                PhoneNumber = "7735457827",
                StreetAddress = "3435 N Central Ave",
                City = "Chicago",
                State = "IL",
                PostalCode = "60634"
            };

            _userManager.CreateAsync(user: adminUser, password: "Admin123*").GetAwaiter().GetResult();

            ApplicationUser? adminUserDb = _db.ApplicationUsers.FirstOrDefault(predicate: u => u.Email == "testadmin@gmail.com");
            if (adminUserDb is not null)
            {
                _userManager.AddToRoleAsync(user: adminUserDb, role: SD.Role_Admin).GetAwaiter().GetResult();
            }
        }

        return;
    }
}
