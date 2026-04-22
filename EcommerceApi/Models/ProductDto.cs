namespace EcommerceApi.Models;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    decimal Price)
{
    public static ProductDto FromProduct(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price);
}
