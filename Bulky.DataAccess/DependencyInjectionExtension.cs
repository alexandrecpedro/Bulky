using Bulky.DataAccess.Data;
using Bulky.DataAccess.DbInitializer.Interfaces;
using Bulky.DataAccess.Extensions;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Emails;
using Bulky.Utility;
using Bulky.Utility.Factories;
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
        ConfigureEmailAddressOrigin(services: services, configuration: configuration);
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

    private static void ConfigureEmailAddressOrigin(this IServiceCollection services, IConfiguration configuration)
    {
        // FLUENT EMAIL SETTINGS 
        //    var EMAIL_ADDRESS_ORIGIN = configuration.GetConnectionString("EmailAddress:Origin") ?? throw new InvalidOperationException("Connection string 'EmailAddress Origin' not found!");

        //    //FluentEmail Services
        //    services
        //        .AddFluentEmail(EMAIL_ADDRESS_ORIGIN)
        //        .AddRazorRenderer()
        //        .AddSmtpSender("localhost", 25);

        // MAILKIT SETTINGS 
        var smtpSettings = new SmtpSettings(
            SmtpServer: configuration.GetValue<string>("EmailSender:MailKit:SmtpServer") ?? throw new InvalidOperationException("SMTP server not found!"),
            SmtpPort: configuration.GetValue<int>("EmailSender:MailKit:SmtpPort"),
            SmtpUser: configuration.GetValue<string>("EmailSender:MailKit:SmtpUser") ?? throw new InvalidOperationException("SMTP username not found!"),
            SmtpPass: configuration.GetValue<string>("EmailSender:MailKit:SmtpPass") ?? throw new InvalidOperationException("SMTP password not found!"),
            EmailFrom: configuration.GetValue<string>("EmailSender:MailKit:Origin") ?? throw new InvalidOperationException("Email address origin not found!")
        );

        services.AddSingleton(implementationInstance: smtpSettings);

        // SEND GRID SETTINGS
        //var sendGridSettings = new SendGridSettings(
        //    ApiKey: configuration.GetValue<string>("EmailSender:SendGrid:SecretKey") ?? throw new InvalidOperationException("SendGrid API Key not found!"),
        //    EmailFrom: configuration.GetValue<string>("EmailSender:SendGrid:Origin") ?? throw new InvalidOperationException("SendGrid email origin not found!")
        //);

        //services.AddSingleton(implementationInstance: sendGridSettings);
    }

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
        services
            .AddSingleton<EmailProviderFactory>()
            .AddScoped<IEmailSender, EmailSender>();

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
        //services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseNpgsql(connectionString: DB_CONNECTION_STRING));

        // MS SQL SERVER
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString: DB_CONNECTION_STRING));
    }
}
