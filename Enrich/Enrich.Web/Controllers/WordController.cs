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
                Categories = await wordService.GetAllCategoriesAsync() ?? new List<Enrich.DAL.Entities.Category>()
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

            var (success, errorMessage) = await wordService.CreatePersonalWordAsync(userId, dto);

            if (!success)
            {
                logger.LogWarning("Помилка створення слова '{Term}' для {UserId}: {Error}", model.Term, userId, errorMessage);
                ModelState.AddModelError(string.Empty, errorMessage!);
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

            var (success, errorMessage) = await wordService.DeleteWordAsync(userId, id);

            if (!success)
            {
                logger.LogWarning("Невдала спроба видалення слова {WordId} користувачем {UserId}: {Error}", id, userId, errorMessage);
                return BadRequest(new { message = errorMessage });
            }

            return Ok();
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
    }
}