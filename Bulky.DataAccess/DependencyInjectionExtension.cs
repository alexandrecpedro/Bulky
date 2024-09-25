using Bulky.DataAccess.Data;
using Bulky.DataAccess.Extensions;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bulky.DataAccess;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //ConfigureIdentity(services: services);
        AddRepositories(services: services);

        if (!configuration.IsTestEnvironment())
        {
            AddDbContext(services: services, configuration: configuration);
        }
    }

    private static void AddRepositories(IServiceCollection services)
    {
        // Category
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // DbInitializer
        //services.AddScoped<IDbInitializer, DbInitializer>();

        // EmailSender
        services.AddScoped<IEmailSender, EmailSender>();

        // Product
        services.AddScoped<IProductRepository, ProductRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var DB_CONNECTION_STRING = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // POSTGRESQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString: DB_CONNECTION_STRING));

        // MS SQL SERVER
        //services.AddDbContext<ApplicationDbContext>(options => 
        //    options.UseSqlServer(connectionString: DB_CONNECTION_STRING));
    }

    //private static void ConfigureIdentity(this IServiceCollection services)
    //{
    //    services.AddIdentity<IdentityUser, IdentityRole>()
    //        .AddEntityFrameworkStores<ApplicationDbContext>()
    //        .AddDefaultTokenProviders();
    //}
}
