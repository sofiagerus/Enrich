using Enrich.BLL.Interfaces;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class CollectionController(
        ILogger<CollectionController> logger,
        IBundleService bundleService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(SystemBundlesIndexViewModel model, int page = 1, int pageSize = 12)
        {
            model.Bundles = await bundleService.GetSystemBundlesAsync(
                model.SearchTerm,
                model.CategoryFilter,
                model.LevelFilter,
                model.MinWordCount,
                model.MaxWordCount,
                page,
                pageSize);

            model.Categories = await bundleService.GetAllCategoriesAsync();

            logger.LogInformation(
                "User {UserId} browsing collections: page={Page}, results={Count}",
                CurrentUserId, page, model.Bundles.Items.Count());

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BundleListPartial", model);
            }

            return View(model);
        }
    }
}
