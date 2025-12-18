using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Models;
using MvcApp.Data;
using System.Data.Common;

namespace MvcApp.Controllers;

public class SearchController : Controller
{
    private readonly ILogger<SearchController> _logger;

    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
    }

       public IActionResult Index(string q)
        {
            var products = ProductStore.Products.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
            if (products == null || products.Count() == 0)
                return View("NotFound");
            return View(products);
        }
    }
