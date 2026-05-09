namespace EcommerceMaui.Models;

public class SearchResponse
{
    public string Query { get; set; } = string.Empty;
    public string? Message { get; set; }
    public List<Product> Results { get; set; } = new();
}
