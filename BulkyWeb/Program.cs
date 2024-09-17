using BulkyWeb;

var builder = WebApplication.CreateBuilder(args);

// Environment
//var environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"];

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddExtensions(configuration: builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//async Task SeedDatabase()
//{
//    using var scope = app.Services.CreateScope();
//    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
//    dbInitializer.Initialize();
//}
