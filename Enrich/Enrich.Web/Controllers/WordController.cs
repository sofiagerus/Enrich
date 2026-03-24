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
        [Authorize]
        public async Task<IActionResult> GetMyWords()
        {
            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                logger.LogWarning("Спроба отримати слова неавторизованим користувачем.");
                return Unauthorized();
            }

            var words = await wordService.GetPersonalWordsAsync(userId);
            logger.LogInformation("Користувач {UserId} отримав {WordCount} слів.", userId, words.Count());
            return Json(words);
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
                logger.LogWarning(
                    "Провалена валідація форми створення слова. Term: {Term}.",
                    model.Term);

                return View(model);
            }

            var userId = userService.GetCurrentUserId(User);
            if (userId == null)
            {
                logger.LogWarning("Спроба створити слово неавторизованим користувачем.");
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
                logger.LogWarning(
                    "Не вдалося створити слово '{Term}' для користувача {UserId}: {Error}",
                    model.Term,
                    userId,
                    errorMessage);

                ModelState.AddModelError(string.Empty, errorMessage!);
                return View(model);
            }

            logger.LogInformation(
                "Користувач {UserId} успішно створив нове слово '{Term}'.",
                userId,
                model.Term);

            TempData["SuccessMessage"] = $"Word '{model.Term}' has been added to your personal dictionary!";
            return RedirectToAction(nameof(MyWords));
        }
    }
}
