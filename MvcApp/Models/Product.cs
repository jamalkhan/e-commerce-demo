namespace MvcApp.Models;

public record Product(
    int Id,
    string Name,
    string Description,
    decimal Price
);
