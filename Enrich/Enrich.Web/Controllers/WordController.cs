using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class WordController(
        ILogger<WordController> logger,
        IWordService wordService,
        IUserService userService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(SystemWordsIndexViewModel model, int page = 1, int pageSize = 12)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            model.Words = await wordService.GetSystemWordsAsync(
                userId,
                model.SearchTerm,
                model.CategoryFilter,
                model.PosFilter,
                model.LevelFilter,
                page,
                pageSize);

            model.Categories = await wordService.GetAllCategoriesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WordListPartial", model);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult MyWords()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var cats = await wordService.GetAllCategoriesAsync();
            var result = cats.Select(c => new { id = c.Id, name = c.Name });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWords(string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page = 1, int pageSize = 20)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                logger.LogWarning("Спроба отримати слова неавторизованим користувачем.");
                return Unauthorized();
            }

            var pageResult = await wordService.GetPersonalWordsAsync(userId, searchTerm, category, partOfSpeech, difficultyLevel, page, pageSize);

            logger.LogInformation("Користувач {UserId} отримав {WordCount} слів (загалом {Total}).", userId, pageResult.Items.Count(), pageResult.TotalCount);
            return Json(pageResult);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateWordViewModel
            {
                // Отримуємо категорії для datalist
                Categories = await wordService.GetAllCategoriesAsync() ?? new List<DAL.Entities.Category>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWordViewModel model)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Провалена валідація форми створення слова для {UserId}.", userId);
                model.Categories = await wordService.GetAllCategoriesAsync();
                return View(model);
            }

            // Обробляємо категорію (знаходимо існуючу або створюємо нову)
            var categoryIds = await HandleCategoryLogic(model.NewCategory);

            var dto = new CreatePersonalWordDTO
            {
                Term = model.Term.Trim(),
                Translation = model.Translation?.Trim(),
                Transcription = model.Transcription?.Trim(),
                Meaning = model.Meaning?.Trim(),
                PartOfSpeech = model.PartOfSpeech?.Trim(),
                Example = model.Example?.Trim(),
                DifficultyLevel = model.DifficultyLevel?.Trim(),
                CategoryIds = categoryIds
            };

            var result = await wordService.CreatePersonalWordAsync(userId, dto);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Помилка створення слова '{Term}' для {UserId}: {Error}", model.Term, userId, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create word.");
                model.Categories = await wordService.GetAllCategoriesAsync();
                return View(model);
            }

            logger.LogInformation("Користувач {UserId} успішно створив слово '{Term}'.", userId, model.Term);
            TempData["SuccessMessage"] = $"Word '{model.Term}' successfully added!";

            return RedirectToAction(nameof(MyWords));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await wordService.DeleteWordAsync(userId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Невдала спроба видалення слова {WordId} користувачем {UserId}: {Error}", id, userId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var word = await wordService.GetPersonalWordForEditAsync(userId, id);

            if (word == null)
            {
                logger.LogWarning("Користувач {UserId} намагався отримати доступ до чужого слова {WordId}", userId, id);
                return NotFound();
            }

            var model = new EditWordViewModel
            {
                WordId = word.Id,
                Term = word.Term,
                Translation = word.Translation,
                Meaning = word.Meaning,
                Example = word.Example
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditWordViewModel model)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new UpdateWordDTO
            {
                WordId = model.WordId,
                Term = model.Term.Trim(),
                Translation = model.Translation?.Trim(),
                Meaning = model.Meaning?.Trim(),
                Example = model.Example?.Trim()
            };

            // Викликаємо сервіс для оновлення
            var success = await wordService.UpdateUserWordAsync(userId, dto);

            if (success)
            {
                logger.LogInformation("Користувач {UserId} успішно оновив слово {WordId}", userId, model.WordId);
                TempData["SuccessMessage"] = "Слово успішно оновлено!";
                return RedirectToAction(nameof(MyWords));
            }

            logger.LogWarning("Невдала спроба оновлення слова {WordId} користувачем {UserId}", model.WordId, userId);
            ModelState.AddModelError(string.Empty, "Ви не маєте прав для редагування цього слова або воно не існує.");
            return View(model);
        }

        private async Task<List<int>> HandleCategoryLogic(string? categoryInput)
        {
            var categoryIds = new List<int>();

            if (!string.IsNullOrWhiteSpace(categoryInput))
            {
                var categoryName = categoryInput.Trim();

                var existingCategory = await wordService.GetCategoryByNameAsync(categoryName);

                if (existingCategory != null)
                {
                    categoryIds.Add(existingCategory.Id);
                }
                else
                {
                    var createdCategory = await wordService.CreateCategoryAsync(categoryName);
                    if (createdCategory != null)
                    {
                        categoryIds.Add(createdCategory.Id);
                    }
                }
            }

            return categoryIds;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSystemWord(int id)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await wordService.SaveSystemWordAsync(userId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Невдала спроба зберегти системне слово {WordId} користувачем {UserId}: {Error}", id, userId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }
    }
}