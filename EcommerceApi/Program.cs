using EcommerceApi.Data;
using EcommerceApi.Models;

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

var app = builder.Build();

app.UseCors("SpaClient");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/products", () =>
{
    var products = ProductStore.Products
        .Select(ProductDto.FromProduct)
        .ToList();

    return Results.Ok(products);
});

app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = ProductStore.Products.FirstOrDefault(p => p.Id == id);

    return product is null
        ? Results.NotFound(new ApiErrorResponse($"Product with id {id} was not found."))
        : Results.Ok(ProductDto.FromProduct(product));
});

app.MapGet("/api/search", (string? q) =>
{
    var query = q?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.Ok(new SearchResponse(
            Query: string.Empty,
            Message: "Enter a product name to search.",
            Results: []));
    }

    var matches = ProductStore.Products
        .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
        .Select(ProductDto.FromProduct)
        .ToList();

    var message = matches.Count == 0
        ? $"No products found for \"{query}\"."
        : null;

    return Results.Ok(new SearchResponse(query, message, matches));
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
