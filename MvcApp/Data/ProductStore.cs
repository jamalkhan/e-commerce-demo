using MvcApp.Models;

namespace MvcApp.Data;

public static class ProductStore
{
    public static readonly List<Product> Products = new()
    {
        new Product(
            Id: 1,
            Name: "Demo Smart Water Bottle",
            Description: "Tracks hydration and syncs with your phone.",
            Price: 49.99m
        )
    };
}
