using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Models;
using MvcApp.Data;

namespace MvcApp.Controllers;

public class ProductController : Controller
{
    // GET /Product
    public IActionResult Index()
    {
        // Return the full list of products
        var products = ProductStore.Products;
        return View(products);
    }

    // GET /Product/Details/{id}
    public IActionResult Details(int id)
    {
        var product = ProductStore.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return View("NotFound"); // Use your NotFound view

        return View(product);
    }
}
