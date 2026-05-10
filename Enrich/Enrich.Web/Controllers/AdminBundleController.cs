using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities.Enums;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Bundles")]
    public class AdminBundleController(
        IBundleService bundleService,
        IWordService wordService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            var pagedResult = await bundleService.GetSystemBundlesAsync(search, null, null, null, null, page, 20);
            ViewBag.SearchTerm = search;
            return View(pagedResult);
        }

        [HttpGet("Community")]
        public async Task<IActionResult> Community(SystemBundlesIndexViewModel model, int page = 1, int pageSize = 12)
        {
            model.PageSize = pageSize;

            model.Bundles = await bundleService.GetCommunityBundlesAsync(
                model.SearchTerm,
                model.CategoryFilter,
                model.LevelFilter,
                model.MinWordCount,
                model.MaxWordCount,
                page,
                pageSize);

            model.Categories = await bundleService.GetAllCategoriesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CommunityBundleListPartial", model);
            }

            return View("CommunityBundles", model);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateBundleViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBundleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var dto = new CreateBundleDTO
            {
                Title = model.Title,
                Description = model.Description,
                DifficultyLevels = model.DifficultyLevels?.ToArray() ?? [],
                ImageUrl = model.ImageUrl,
                CategoryIds = model.CategoryIds,
                WordIds = model.WordIds
            };

            var result = await bundleService.CreateSystemBundleAsync(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create collection.");
                await PopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Collection created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);
            if (bundle == null)
            {
                return NotFound();
            }

            var viewModel = new EditBundleViewModel
            {
                Id = bundle.Id,
                IsSystem = bundle.IsSystem,
                Title = bundle.Title,
                Description = bundle.Description,
                ImageUrl = bundle.ImageUrl,
                Status = Enum.Parse<BundleStatus>(bundle.Status, true),
                CategoryIds = bundle.CategoryIds,
                WordIds = bundle.WordIds,
                DifficultyLevels = bundle.DifficultyLevels?.ToList() ?? []
            };

            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBundleViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var dto = new CreateBundleDTO
            {
                Title = model.Title,
                Description = model.Description,
                DifficultyLevels = model.DifficultyLevels?.ToArray() ?? [],
                ImageUrl = model.ImageUrl,
                CategoryIds = model.CategoryIds,
                WordIds = model.WordIds
            };

            var result = model.IsSystem
                ? await bundleService.UpdateSystemBundleAsync(id, dto)
                : await bundleService.UpdateCommunityBundleAsync(id, dto, model.Status);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update collection.");
                await PopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Collection updated successfully!";

            return model.IsSystem ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Community));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);
            if (bundle != null)
            {
                var result = bundle.IsSystem
                    ? await bundleService.DeleteSystemBundleAsync(id)
                    : await bundleService.DeleteBundleAsync(bundle.OwnerId ?? string.Empty, id);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete the collection.";
                    return bundle.IsSystem ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Community));
                }

                TempData["SuccessMessage"] = "Collection deleted successfully.";

                if (!bundle.IsSystem)
                {
                    return RedirectToAction(nameof(Community));
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to find the collection.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Community/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCommunity(int id)
        {
            try
            {
                var result = await bundleService.DeleteCommunityBundleAsync(id);

                if (result.IsSuccess)
                {
                    return Json(new { success = true, message = "Collection deleted successfully." });
                }

                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage ?? "Failed to delete the collection."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting bundle {id}: {ex.Message}");
                return Json(new { success = false, message = "An internal error occurred while deleting." });
            }
        }

        private async Task PopulateDropdowns(CreateBundleViewModel model)
        {
            var categories = await bundleService.GetAllCategoriesAsync();
            var words = await wordService.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }

        private async Task PopulateDropdowns(EditBundleViewModel model)
        {
            var categories = await bundleService.GetAllCategoriesAsync();
            var words = await wordService.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }
    }
}