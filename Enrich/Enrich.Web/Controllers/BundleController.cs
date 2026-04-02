using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class BundleController(
        ILogger<BundleController> logger,
        IBundleService bundleService,
        ICategoryRepository categoryRepository,
        IWordRepository wordRepository) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            string? categoryFilter = null,
            string? levelFilter = null,
            int? minWordCount = null,
            int? maxWordCount = null,
            int page = 1,
            int pageSize = 6)
        {
            var pagedBundles = await bundleService.GetUserBundlesPageAsync(
                CurrentUserId,
                search,
                categoryFilter,
                levelFilter,
                minWordCount,
                maxWordCount,
                page,
                pageSize);

            logger.LogInformation(
                "Користувач {UserId} переглянув сторінку {Page} своїх бандлів. Пошук: '{Search}', Категорії: '{Categories}', Рівні: '{Levels}', Слів: {MinWords}-{MaxWords}",
                CurrentUserId,
                page,
                search ?? "(немає)",
                categoryFilter ?? "(немає)",
                levelFilter ?? "(немає)",
                minWordCount ?? 0,
                maxWordCount ?? 0);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MyBundleListPartial", pagedBundles);
            }

            var categories = await categoryRepository.GetAllCategoriesAsync();

            var viewModel = new BundleIndexViewModel
            {
                Bundles = pagedBundles,
                SearchTerm = search,
                LevelFilter = levelFilter,
                MinWordCount = minWordCount,
                MaxWordCount = maxWordCount,
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            var availableLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };

            var viewModel = new CreateBundleViewModel
            {
                Categories = categories.Select(c => (c.Id, c.Name)).ToList(),
                Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList(),
                AvailableLevels = availableLevels
            };

            logger.LogInformation("Користувач {UserId} відкрив форму створення бандлу.", CurrentUserId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBundleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                logger.LogWarning(
                    "Провалена валідація форми створення бандлу для користувача {UserId}. Помилки: {Errors}",
                    CurrentUserId,
                    errors);

                var categories = await categoryRepository.GetAllCategoriesAsync();
                var words = await wordRepository.GetAllWordsAsync();
                var availableLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };

                model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
                model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
                model.AvailableLevels = availableLevels;

                return View(model);
            }

            var dto = new CreateBundleDTO
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                DifficultyLevels = model.DifficultyLevels?.Any() == true ? model.DifficultyLevels.ToArray() : [],
                ImageUrl = model.ImageUrl,
                CategoryIds = model.CategoryIds?.Any() == true ? model.CategoryIds : null,
                WordIds = model.WordIds?.Any() == true ? model.WordIds : null
            };

            var result = await bundleService.CreateBundleAsync(CurrentUserId, dto);

            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Помилка створення бандлу '{Title}' для користувача {UserId}: {Error}",
                    model.Title,
                    CurrentUserId,
                    result.ErrorMessage);

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не вдалося створити бандл.");

                var categories = await categoryRepository.GetAllCategoriesAsync();
                var words = await wordRepository.GetAllWordsAsync();
                var availableLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };

                model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
                model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
                model.AvailableLevels = availableLevels;

                return View(model);
            }

            logger.LogInformation(
                "Бандл '{Title}' успішно створено користувачем {UserId}.",
                model.Title,
                CurrentUserId);

            TempData["SuccessMessage"] = $"Бандл '{model.Title}' успішно створено!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);

            if (bundle == null)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував редагувати неіснуючий бандл {BundleId}.",
                    CurrentUserId,
                    id);

                return NotFound();
            }

            if (bundle.OwnerId != CurrentUserId)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував редагувати чужий бандл {BundleId}.",
                    CurrentUserId,
                    id);

                return Forbid();
            }

            var viewModel = new EditBundleViewModel
            {
                Id = bundle.Id,
                Title = bundle.Title,
                Description = bundle.Description,
                ImageUrl = bundle.ImageUrl,
                Status = Enum.Parse<BundleStatus>(bundle.Status, ignoreCase: true),
                CategoryIds = bundle.CategoryIds,
                WordIds = bundle.WordIds,
                DifficultyLevels = bundle.DifficultyLevels?.ToList() ?? []
            };

            var categories = await categoryRepository.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            var availableLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };

            viewModel.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            viewModel.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            viewModel.AvailableLevels = availableLevels;

            logger.LogInformation(
                "Користувач {UserId} відкрив форму редагування бандлу {BundleId}.",
                CurrentUserId,
                id);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBundleViewModel model)
        {
            if (id != model.Id)
            {
                logger.LogWarning(
                    "ID невідповідності при редагуванні бандлу для користувача {UserId}.",
                    CurrentUserId);

                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                logger.LogWarning(
                    "Провалена валідація форми редагування бандлу {BundleId} для користувача {UserId}. Помилки: {Errors}",
                    id,
                    CurrentUserId,
                    errors);

                var categories = await categoryRepository.GetAllCategoriesAsync();
                var words = await wordRepository.GetAllWordsAsync();
                var availableLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };

                model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
                model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
                model.AvailableLevels = availableLevels;

                return View(model);
            }

            var dto = new CreateBundleDTO
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                DifficultyLevels = model.DifficultyLevels?.Any() == true ? model.DifficultyLevels.ToArray() : [],
                ImageUrl = model.ImageUrl,
                CategoryIds = model.CategoryIds?.Any() == true ? model.CategoryIds : null,
                WordIds = model.WordIds?.Any() == true ? model.WordIds : null
            };

            var result = await bundleService.UpdateBundleAsync(CurrentUserId, id, dto);

            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Помилка редагування бандлу {BundleId} користувачем {UserId}: {Error}",
                    id,
                    CurrentUserId,
                    result.ErrorMessage);

                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не вдалося оновити бандл.");
                var categories = await categoryRepository.GetAllCategoriesAsync();
                var words = await wordRepository.GetAllWordsAsync();
                var availableLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };

                model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
                model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
                model.AvailableLevels = availableLevels;
                return View(model);
            }

            logger.LogInformation(
                "Бандл {BundleId} успішно оновлено користувачем {UserId}.",
                id,
                CurrentUserId);

            TempData["SuccessMessage"] = "Бандл успішно оновлено!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await bundleService.DeleteBundleAsync(CurrentUserId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Помилка видалення бандлу {BundleId} користувачем {UserId}: {Error}",
                    id,
                    CurrentUserId,
                    result.ErrorMessage);

                TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося видалити бандл.";
                return RedirectToAction("Index");
            }

            logger.LogInformation(
                "Бандл {BundleId} успішно видалено користувачем {UserId}.",
                id,
                CurrentUserId);

            TempData["SuccessMessage"] = "Бандл успішно видалено!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyBundles(
            string? searchTerm,
            string? categoryFilter = null,
            string? levelFilter = null,
            int? minWordCount = null,
            int? maxWordCount = null,
            int page = 1,
            int pageSize = 20)
        {
            var pageResult = await bundleService.GetUserBundlesPageAsync(
                CurrentUserId,
                searchTerm,
                categoryFilter,
                levelFilter,
                minWordCount,
                maxWordCount,
                page,
                pageSize);

            logger.LogInformation(
                "Користувач {UserId} отримав {BundleCount} бандлів (всього {Total}).",
                CurrentUserId,
                pageResult.Items.Count(),
                pageResult.TotalCount);

            return Json(pageResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWords(int bundleId, [FromBody] IEnumerable<int> wordIds)
        {
            if (!wordIds.Any())
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував додати порожний список слів до бандлу {BundleId}.",
                    CurrentUserId,
                    bundleId);

                return BadRequest("Будь ласка, виберіть щонайменше одне слово.");
            }

            var result = await bundleService.AddWordsToBundleAsync(CurrentUserId, bundleId, wordIds);

            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Помилка додавання слів до бандлу {BundleId} користувачем {UserId}: {Error}",
                    bundleId,
                    CurrentUserId,
                    result.ErrorMessage);

                return BadRequest(result.ErrorMessage);
            }

            logger.LogInformation(
                "Користувач {UserId} успішно додав {Count} слів до бандлу {BundleId}.",
                CurrentUserId,
                wordIds.Count(),
                bundleId);

            return Ok("Слова успішно додано до бандлу!");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveWords(int bundleId, [FromBody] IEnumerable<int> wordIds)
        {
            if (!wordIds.Any())
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував видалити порожний список слів з бандлу {BundleId}.",
                    CurrentUserId,
                    bundleId);

                return BadRequest("Будь ласка, виберіть щонайменше одне слово.");
            }

            var result = await bundleService.RemoveWordsFromBundleAsync(CurrentUserId, bundleId, wordIds);

            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Помилка видалення слів з бандлу {BundleId} користувачем {UserId}: {Error}",
                    bundleId,
                    CurrentUserId,
                    result.ErrorMessage);

                return BadRequest(result.ErrorMessage);
            }

            logger.LogInformation(
                "Користувач {UserId} успішно видалив {Count} слів з бандлу {BundleId}.",
                CurrentUserId,
                wordIds.Count(),
                bundleId);

            return Ok("Слова успішно видалено з бандлу!");
        }

        [HttpPost]
        public async Task<IActionResult> SaveSystemBundle(int id)
        {
            var result = await bundleService.SaveSystemBundleAsync(CurrentUserId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Невдала спроба зберегти системний бандл {BundleId} користувачем {UserId}: {Error}", id, CurrentUserId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }
    }
}
