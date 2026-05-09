using EcommerceData.Repositories;
using EcommerceMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMvc.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _products;

    public ProductController(IProductRepository products)
    {
        _products = products;
    }

    // GET /Product
    public async Task<IActionResult> Index()
    {
        var entities = await _products.GetAllAsync();
        var view = entities
            .Select(p => new Product(p.Id, p.Name, p.Description, p.Price))
            .ToList();
        return View(view);
    }

    // GET /Product/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var entity = await _products.GetByIdAsync(id);
        if (entity is null)
            return View("NotFound");

        return View(new Product(entity.Id, entity.Name, entity.Description, entity.Price));
    }
}
