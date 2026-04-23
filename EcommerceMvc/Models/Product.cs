namespace EcommerceMvc.Models;

public record Product(
    int Id,
    string Name,
    string Description,
    decimal Price
);
