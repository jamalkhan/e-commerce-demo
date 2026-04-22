namespace EcommerceApi.Models;

public record SearchResponse(
    string Query,
    string? Message,
    IReadOnlyList<ProductDto> Results
);
