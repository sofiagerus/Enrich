using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Bundles")]
    public class AdminBundleController(
        IBundleService bundleService,
        ICategoryRepository categoryRepository,
        IWordRepository wordRepository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            var pagedResult = await bundleService.GetSystemBundlesAsync(search, null, null, null, null, page, 20);
            ViewBag.SearchTerm = search;
            return View(pagedResult);
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
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка створення.");
                await PopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Системний бандл успішно створено!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);
            if (bundle == null || !bundle.IsSystem)
            {
                return NotFound();
            }

            var viewModel = new EditBundleViewModel
            {
                Id = bundle.Id,
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

            var result = await bundleService.UpdateSystemBundleAsync(id, dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка оновлення.");
                await PopulateDropdowns(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Системний бандл успішно оновлено!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);
            if (bundle != null && bundle.IsSystem)
            {
                await bundleService.DeleteBundleAsync(bundle.OwnerId ?? "SYSTEM", id);
                TempData["SuccessMessage"] = "Бандл видалено.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(CreateBundleViewModel model)
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }

        private async Task PopulateDropdowns(EditBundleViewModel model)
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }
    }
}