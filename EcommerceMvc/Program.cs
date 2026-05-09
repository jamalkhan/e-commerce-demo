using EcommerceData.Data;
using EcommerceData.Seeding;
using EcommerceData.SqlServer.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var dbProvider = builder.Configuration["Database:Provider"]
    ?? throw new InvalidOperationException("Configuration 'Database:Provider' is required.");

var connectionString = builder.Configuration.GetConnectionString("EcommerceDb")
    ?? throw new InvalidOperationException("Connection string 'EcommerceDb' is required.");

switch (dbProvider)
{
    case "SqlServer":
        builder.Services.AddSqlServerEcommerceData(connectionString);
        break;
    default:
        throw new InvalidOperationException(
            $"Database provider '{dbProvider}' is not supported. Add a provider package and update Program.cs.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<ProductSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program;
