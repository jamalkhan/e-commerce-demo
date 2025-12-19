using Microsoft.AspNetCore.Mvc;

namespace MvcApp.Controllers;

public class LoginController : Controller
{
    private readonly ILogger<SearchController> _logger;

    public LoginController(ILogger<SearchController> logger)
    {
        _logger = logger;
    }

       public IActionResult Index(string uname)
    {
        return View();
    }
}
