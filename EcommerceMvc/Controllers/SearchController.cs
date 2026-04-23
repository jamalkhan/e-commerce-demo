using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcommerceMvc.Models;
using EcommerceMvc.Data;
using System.Data.Common;
using System.Linq;

namespace EcommerceMvc.Controllers;

public class SearchController : Controller
{
    private readonly ILogger<SearchController> _logger;

    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
    }

       public IActionResult Index(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            ViewData["Message"] = "Enter a product name to search.";
            return View(Enumerable.Empty<Product>());
        }

        var products = ProductStore.Products
            .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ViewData["Query"] = q;
        return View(products);
    }
}
