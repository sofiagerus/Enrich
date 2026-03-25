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
        public IActionResult Create()
        {
            return View(new CreateWordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Провалена валідація форми створення слова. Term: {Term}.", model.Term);
                return View(model);
            }

            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var dto = new CreatePersonalWordDTO
            {
                Term = model.Term,
                Translation = model.Translation,
                Transcription = model.Transcription,
                Meaning = model.Meaning,
                PartOfSpeech = model.PartOfSpeech,
                Example = model.Example,
                DifficultyLevel = model.DifficultyLevel,
            };

            var (success, errorMessage) = await wordService.CreatePersonalWordAsync(userId, dto);

            if (!success)
            {
                logger.LogWarning("Помилка створення слова '{Term}' для {UserId}: {Error}", model.Term, userId, errorMessage);
                ModelState.AddModelError(string.Empty, errorMessage!);
                return View(model);
            }

            logger.LogInformation("Користувач {UserId} створив слово '{Term}'.", userId, model.Term);
            TempData["SuccessMessage"] = $"Word '{model.Term}' added!";
            return RedirectToAction(nameof(MyWords));
        }

        // Новий метод видалення
        [HttpPost]

        // [ValidateAntiForgeryToken] // Розкоментуйте, якщо додасте токен у fetch запит на фронтенді
        public async Task<IActionResult> Delete(int id)
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                logger.LogWarning("Анонімна спроба видалення слова ID: {WordId}", id);
                return Unauthorized();
            }

            // Викликаємо сервіс. В сервісі має бути перевірка:
            // якщо слово створене юзером - видаляємо з БД.
            // якщо слово системне - видаляємо лише запис із таблиці зв'язків UserWords.
            var (success, errorMessage) = await wordService.DeleteWordAsync(userId, id);

            if (!success)
            {
                logger.LogWarning("Невдала спроба видалення слова {WordId} користувачем {UserId}: {Error}", id, userId, errorMessage);
                return BadRequest(new { message = errorMessage });
            }

            logger.LogInformation("Користувач {UserId} видалив слово {WordId}.", userId, id);
            return Ok();
        }
    }
}