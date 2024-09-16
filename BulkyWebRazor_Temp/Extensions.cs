using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;

namespace BulkyWebRazor_Temp;

public static class Extensions
{
    public static void AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration
   )
    {
        if (!configuration.IsTestEnvironment())
        {
            services.AddDbContext(configuration: configuration);
        }

        services.AddRazorPages();
        //services.ConfigureStripe(configuration: configuration);
        services.ConfigureIdentity();
        //services.ConfigureAuthentication(configuration: configuration);
        services.ConfigureCookies();
        services.AddSessionConfiguration();
        services.AddRateLimiter();
    }
    public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var DB_CONNECTION_STRING = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString: DB_CONNECTION_STRING));
    }

    public static bool IsTestEnvironment(this IConfiguration configuration)
        => configuration.GetValue<bool>("InMemoryTest");

    //public static void ConfigureStripe(this IServiceCollection services, IConfiguration configuration)
    //{
    //    services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
    //    StripeConfiguration.ApiKey = configuration.GetSection("Stripe:SecretKey").Get<string>();
    //}

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    }

    //public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    //{
    //    services.AddAuthentication().AddFacebook(option => {
    //        option.AppId = configuration["Authentication:Facebook:AppId"];
    //        option.AppSecret = configuration["Authentication:Facebook:AppSecret"];
    //    });

    //    services.AddAuthentication().AddMicrosoftAccount(option => {
    //        option.ClientId = configuration["Authentication:Microsoft:ClientId"];
    //        option.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
    //    });
    //}

    public static void ConfigureCookies(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options => {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        });
    }

    public static void AddSessionConfiguration(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options => {
            options.IdleTimeout = TimeSpan.FromMinutes(100);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(rateLimiterOptions =>
        {
            // (A) For all users - Token
            //rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            //rateLimiterOptions.AddTokenBucketLimiter("token", options =>
            //{
            //    options.TokenLimit = 1000;
            //    options.ReplenishmentPeriod = TimeSpan.FromHours(1);
            //    options.TokensPerPeriod = 700;
            //    options.AutoReplenishment = true;
            //});

            // (B) For all users - fixed limit
            // rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            // {
            //    options.Window = TimeSpan.FromMinutes(1);
            //    options.PermitLimit = 5;
            //    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            //    options.QueueLimit = 5;
            // });

            // (C) By IP Address
            rateLimiterOptions.AddPolicy("fixed-by-ip", httpContext =>
            {
                var userIp = httpContext.Connection.RemoteIpAddress?.ToString();

                // Check if 'X-Forwarded-For' header exists
                if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
                {
                    userIp = value.FirstOrDefault();
                }

                // Get HTTP request method path
                var endpointKey = httpContext.Request.Path.ToString();

                var partitionKey = $"{userIp}:{endpointKey}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
