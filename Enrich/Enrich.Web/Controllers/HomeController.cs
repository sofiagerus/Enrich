using System.Diagnostics;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        public IActionResult Index()
        {
            logger.LogInformation("Відкрито головну сторінку");
            return View();
        }

        public IActionResult Privacy()
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