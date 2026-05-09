using EcommerceApi.Models;
using EcommerceData.Data;
using EcommerceData.Repositories;
using EcommerceData.Seeding;
using EcommerceData.SqlServer.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaClient", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy.SetIsOriginAllowed(origin =>
            allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
            (builder.Environment.IsDevelopment() && IsLocalDevelopmentOrigin(origin)))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseCors("SpaClient");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<ProductSeeder>();
    await seeder.SeedAsync();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/products", async (IProductRepository repo) =>
{
    var products = await repo.GetAllAsync();
    var dtos = products.Select(ProductDto.FromProduct).ToList();
    return Results.Ok(dtos);
});

app.MapGet("/api/products/{id:int}", async (int id, IProductRepository repo) =>
{
    var product = await repo.GetByIdAsync(id);
    return product is null
        ? Results.NotFound(new ApiErrorResponse($"Product with id {id} was not found."))
        : Results.Ok(ProductDto.FromProduct(product));
});

app.MapGet("/api/search", async (string? q, IProductRepository repo) =>
{
    var query = q?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.Ok(new SearchResponse(
            Query: string.Empty,
            Message: "Enter a product name to search.",
            Results: []));
    }

    var matches = await repo.SearchByNameAsync(query);
    var dtos = matches.Select(ProductDto.FromProduct).ToList();
    var message = dtos.Count == 0 ? $"No products found for \"{query}\"." : null;
    return Results.Ok(new SearchResponse(query, message, dtos));
});

app.MapPost("/api/login", (LoginRequest request) => Login(request.UserName));
app.MapGet("/api/login", (string? uname) => Login(uname));

app.Run();

static IResult Login(string? userName)
{
    if (string.IsNullOrWhiteSpace(userName))
    {
        return Results.BadRequest(new ApiErrorResponse("Enter a username to log in."));
    }

    return Results.Ok(new LoginResponse(
        UserName: userName.Trim(),
        Message: "You have successfully logged in."));
}

static bool IsLocalDevelopmentOrigin(string origin)
{
    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    return uri.Scheme is "http" or "https" &&
        (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
         uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase));
}

public partial class Program;
