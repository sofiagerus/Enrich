using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enrich.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(
        IUserService userService,
        IBundleService bundleService,
        IOptions<PaginationSettings> paginationOptions,
        ILogger<AdminController> logger) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Browse()
        {
            logger.LogInformation("Administrator views the list of all users.");

            var usersDto = await userService.GetAllUsersAsync();
            var model = usersDto.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = "User",
                IsLockedOut = u.IsLockedOut
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Restrict(string id, string username)
        {
            var model = new RestrictAccountViewModel
            {
                UserId = id,
                Username = username
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restrict(RestrictAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new RestrictAccountDTO
            {
                UserId = model.UserId,
                Reason = model.Reason
            };

            var result = await userService.RestrictUserAsync(dto);

            if (result.IsSuccess)
            {
                logger.LogInformation("Administrator blocked account {UserId}", model.UserId);
                TempData["SuccessMessage"] = $"User account {model.Username} successfully blocked.";
                return RedirectToAction("Browse");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to restrict account.");

            return View(model);
        }

        [HttpGet]
        public IActionResult Restore(string id, string username)
        {
            var model = new RestoreAccountViewModel
            {
                UserId = id,
                Username = username
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(RestoreAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new RestoreAccountDTO
            {
                UserId = model.UserId
            };

            var result = await userService.RestoreUserAsync(dto);

            if (result.IsSuccess)
            {
                logger.LogInformation("Administrator unblocked account {UserId}", model.UserId);
                TempData["SuccessMessage"] = $"User account {model.Username} successfully unblocked.";
                return RedirectToAction("Browse");
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to restore account.");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PendingBundles(SystemBundlesIndexViewModel model, int page = 1, int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = paginationOptions.Value.DefaultSystemBundlesPageSize;
            }

            model.PageSize = pageSize;

            model.Bundles = await bundleService.GetPendingBundlesAsync(
                model.SearchTerm,
                model.CategoryFilter,
                model.LevelFilter,
                model.MinWordCount,
                model.MaxWordCount,
                page,
                pageSize);

            model.Categories = await bundleService.GetAllCategoriesAsync();

            logger.LogInformation(
                "Admin {UserId} browsing pending bundles: page={Page}, results={Count}",
                CurrentUserId, page, model.Bundles.Items.Count());

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_BundleListPartial", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReviewBundle(int id, string decision)
        {
            if (string.IsNullOrWhiteSpace(decision) || (decision != "publish" && decision != "reject"))
            {
                return BadRequest(new { message = "Invalid action." });
            }

            bool approve = decision == "publish";
            var result = await bundleService.ReviewBundleAsync(id, approve);

            if (result.IsSuccess)
            {
                return Ok(new { message = $"Bundle successfully {decision}ed!" });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }
    }
}