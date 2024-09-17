using Bulky.DataAccess.Data;
using Bulky.DataAccess.Extensions;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
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
        AddRepositories(services: services);

        if (!configuration.IsTestEnvironment())
        {
            services.AddDbContext(configuration: configuration);
        }
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services
            .AddScoped<ICategoryRepository, CategoryRepository>();
    }

    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var DB_CONNECTION_STRING = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString: DB_CONNECTION_STRING));
    }
}
