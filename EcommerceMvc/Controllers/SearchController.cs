using EcommerceData.Repositories;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMvc.Controllers;

public class SearchController : Controller
{
    private readonly ILogger<SearchController> _logger;
    private readonly IProductRepository _products;

    public SearchController(ILogger<SearchController> logger, IProductRepository products)
    {
        _logger = logger;
        _products = products;
    }

    public async Task<IActionResult> Index(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            ViewData["Message"] = "Enter a product name to search.";
            return View(Enumerable.Empty<Product>());
        }

        var entities = await _products.SearchByNameAsync(q);
        var view = entities
            .Select(p => new Product(p.Id, p.Name, p.Description, p.Price))
            .ToList();

        ViewData["Query"] = q;
        return View(view);
    }
}
