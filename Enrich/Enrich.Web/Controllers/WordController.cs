using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class WordController(
        ILogger<WordController> logger,
        IWordService wordService,
        IOptions<PaginationSettings> paginationOptions) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(SystemWordsIndexViewModel model, int page = 1, int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = paginationOptions.Value.DefaultSystemWordsPageSize;
            }

            model.PageSize = pageSize;
            model.Words = await wordService.GetSystemWordsAsync(
                CurrentUserId,
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
            ViewData["PageSize"] = paginationOptions.Value.DefaultPersonalWordsPageSize;
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
        public async Task<IActionResult> GetMyWords(string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page = 1, int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = paginationOptions.Value.DefaultPersonalWordsPageSize;
            }

            var pageResult = await wordService.GetPersonalWordsAsync(CurrentUserId, searchTerm, category, partOfSpeech, difficultyLevel, page, pageSize);

            logger.LogInformation("User {UserId} retrieved {WordCount} words (total {Total}).", CurrentUserId, pageResult.Items.Count(), pageResult.TotalCount);
            return Json(pageResult);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateWordViewModel
            {
                // Retrieve categories for datalist
                Categories = await wordService.GetAllCategoriesAsync() ?? new List<DAL.Entities.Category>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Create word form validation failed for {UserId}.", CurrentUserId);
                model.Categories = await wordService.GetAllCategoriesAsync();
                return View(model);
            }

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

            var result = await wordService.CreatePersonalWordAsync(CurrentUserId, dto);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Error creating word '{Term}' for {UserId}: {Error}", model.Term, CurrentUserId, result.ErrorMessage);
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create word.");
                model.Categories = await wordService.GetAllCategoriesAsync();
                return View(model);
            }

            logger.LogInformation("User {UserId} successfully created word '{Term}'.", CurrentUserId, model.Term);
            TempData["SuccessMessage"] = $"Word '{model.Term}' successfully added!";

            return RedirectToAction(nameof(MyWords));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await wordService.DeleteWordAsync(CurrentUserId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Failed attempt to delete word {WordId} by user {UserId}: {Error}", id, CurrentUserId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await wordService.GetPersonalWordForEditAsync(CurrentUserId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("User {UserId} attempted to access word {WordId}: {Error}", CurrentUserId, id, result.ErrorMessage);
                return NotFound();
            }

            var word = result.Value!;
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

            var result = await wordService.UpdateUserWordAsync(CurrentUserId, dto);

            if (result.IsSuccess)
            {
                logger.LogInformation("User {UserId} successfully updated word {WordId}", CurrentUserId, model.WordId);
                TempData["SuccessMessage"] = "Word successfully updated!";
                return RedirectToAction(nameof(MyWords));
            }

            logger.LogWarning("Failed attempt to update word {WordId} by user {UserId}: {Error}", model.WordId, CurrentUserId, result.ErrorMessage);
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update word.");
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

        [HttpPost("Words/AddToMyWords")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToMyWords([FromBody] SaveWordRequest request)
        {
            if (request == null || request.WordId <= 0)
            {
                logger.LogWarning("User {UserId} submitted an invalid save word request.", CurrentUserId);
                return BadRequest(new { message = "Invalid word data." });
            }

            var result = await wordService.SaveWordToLibraryAsync(CurrentUserId, request.WordId);
            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "User {UserId} failed to save word {WordId} from study: {Error}.",
                    CurrentUserId,
                    request.WordId,
                    result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Word saved to your library." });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSystemWord(int id)
        {
            var result = await wordService.SaveSystemWordAsync(CurrentUserId, id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Failed attempt to save system word {WordId} by user {UserId}: {Error}", id, CurrentUserId, result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }
    }
}
