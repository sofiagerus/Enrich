using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class CollectionController(
        ILogger<CollectionController> logger,
        IBundleService bundleService,
        IOptions<PaginationSettings> paginationOptions) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(SystemBundlesIndexViewModel model, int page = 1, int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = paginationOptions.Value.DefaultSystemBundlesPageSize;
            }

            model.PageSize = pageSize;
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
