using Bulky.DataAccess.Data;
using Bulky.DataAccess.DbInitializer.Interfaces;
using Bulky.DataAccess.Extensions;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
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
        ConfigureIdentity(services: services);
        //ConfigureEmailAddressOrigin(services: services, configuration: configuration);
        AddRepositories(services: services);

        if (!configuration.IsTestEnvironment())
        {
            AddDbContext(services: services, configuration: configuration);
        }
    }

    private static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    }

    // FLUENT EMAIL LIBRARY
    //private static void ConfigureEmailAddressOrigin(this IServiceCollection services, IConfiguration configuration)
    //{
    //    var EMAIL_ADDRESS_ORIGIN = configuration.GetConnectionString("EmailAddress:Origin") ?? throw new InvalidOperationException("Connection string 'EmailAddress Origin' not found!");

    //    //FluentEmail Services
    //    services
    //        .AddFluentEmail(EMAIL_ADDRESS_ORIGIN)
    //        .AddRazorRenderer()
    //        .AddSmtpSender("localhost", 25);
    //}

    private static void AddRepositories(IServiceCollection services)
    {
        // ApplicationUser
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

        // Category
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Company
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // DbInitializer
        services.AddScoped<IDbInitializer, DbInitializer.DbInitializer>();

        // EmailSender
        services.AddScoped<IEmailSender, EmailSender>();

        // OrderDetail
        services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

        // OrderHeader
        services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();

        // Product
        services.AddScoped<IProductRepository, ProductRepository>();

        // Shopping Cart
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var DB_CONNECTION_STRING = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found!");

        // POSTGRESQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString: DB_CONNECTION_STRING));

        // MS SQL SERVER
        //services.AddDbContext<ApplicationDbContext>(options => 
        //    options.UseSqlServer(connectionString: DB_CONNECTION_STRING));
    }
}
