using System.Diagnostics;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : BaseController
    {
        public IActionResult Index()
        {
            logger.LogInformation("Home page opened");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RateLimitExceeded()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = id
            });
        }
    }
}