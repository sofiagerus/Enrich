using System.Text.Json;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enrich.Web.Controllers
{
    [Authorize]
    public class BundleController(
        ILogger<BundleController> logger,
        IBundleService bundleService,
        IWordRepository wordRepository,
        IStudySessionService studySessionService,
        IOptions<PaginationSettings> paginationOptions) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            string? categoryFilter = null,
            string? levelFilter = null,
            int? minWordCount = null,
            int? maxWordCount = null,
            int page = 1,
            int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = paginationOptions.Value.DefaultUserBundlesPageSize;
            }

            var pagedBundles = await bundleService.GetUserBundlesPageAsync(
                CurrentUserId, search, categoryFilter, levelFilter,
                minWordCount, maxWordCount, page, pageSize);

            logger.LogInformation(
                "User {UserId} viewed page {Page} of their bundles.",
                CurrentUserId, page);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MyBundleListPartial", pagedBundles);
            }

            var categories = await bundleService.GetAllCategoriesAsync();

            var viewModel = new BundleIndexViewModel
            {
                Bundles = pagedBundles,
                SearchTerm = search,
                LevelFilter = levelFilter,
                MinWordCount = minWordCount,
                MaxWordCount = maxWordCount,
                Categories = categories,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);

            if (bundle == null)
            {
                logger.LogWarning("User {UserId} attempted to view a non-existent collection {BundleId}.", CurrentUserId, id);
                return NotFound();
            }

            if (!bundle.IsSystem && bundle.OwnerId != CurrentUserId && !bundle.IsPublic)
            {
                logger.LogWarning("Access denied: User {UserId} attempted to view private collection {BundleId}.", CurrentUserId, id);
                return Forbid();
            }

            logger.LogInformation("User {UserId} is viewing collection {BundleId} (\"{Title}\").", CurrentUserId, id, bundle.Title);

            return View(bundle);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await bundleService.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();

            var viewModel = new CreateBundleViewModel
            {
                Categories = categories.Select(c => (c.Id, c.Name)).ToList(),
                Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList(),
                AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"]
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBundleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ReloadCreateViewModelData(model);
                return View(model);
            }

            var dto = new CreateBundleDTO
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                DifficultyLevels = model.DifficultyLevels?.ToArray() ?? [],
                ImageUrl = model.ImageUrl,
                CategoryIds = model.CategoryIds,
                WordIds = model.WordIds
            };

            var result = await bundleService.CreateBundleAsync(CurrentUserId, dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Creation error.");
                await ReloadCreateViewModelData(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Bundle '{model.Title}' successfully created!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bundle = await bundleService.GetBundleByIdAsync(id);
            if (bundle == null)
            {
                return NotFound();
            }

            if (bundle.OwnerId != CurrentUserId)
            {
                return Forbid();
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

            await ReloadEditViewModelData(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditBundleViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await ReloadEditViewModelData(model);
                return View(model);
            }

            var dto = new CreateBundleDTO
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                DifficultyLevels = model.DifficultyLevels?.ToArray() ?? [],
                ImageUrl = model.ImageUrl,
                CategoryIds = model.CategoryIds,
                WordIds = model.WordIds
            };

            var result = await bundleService.UpdateBundleAsync(CurrentUserId, id, dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Update error.");
                await ReloadEditViewModelData(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Bundle successfully updated!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Study(int bundleId)
        {
            var result = await studySessionService.StartStudySessionAsync(CurrentUserId, bundleId);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to start learning.";
                return RedirectToAction("Index");
            }

            var sessionDto = result.Value!;
            var viewModel = new StudySessionViewModel
            {
                SessionId = sessionDto.SessionId,
                BundleId = sessionDto.BundleId,
                BundleTitle = sessionDto.BundleTitle,
                Cards = sessionDto.Cards.Select(c => new StudyCardViewModel
                {
                    WordId = c.WordId,
                    Term = c.Term,
                    Translation = c.Translation,
                    Transcription = c.Transcription,
                    Meaning = c.Meaning,
                    PartOfSpeech = c.PartOfSpeech,
                    Example = c.Example
                }).ToList(),
                TotalCards = sessionDto.TotalCards,
                StartedAt = sessionDto.StartedAt
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDTO dto)
        {
            var result = await studySessionService.SubmitAnswerAsync(CurrentUserId, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Json(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> FinishSession([FromBody] FinishSessionRequest request)
        {
            var result = await studySessionService.FinishStudySessionAsync(CurrentUserId, request.SessionId);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { redirectUrl = Url.Action("Index", "Bundle") });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSystemBundle(int id)
        {
            var result = await bundleService.SaveSystemBundleAsync(CurrentUserId, id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SaveCommunityBundle(int id)
        {
            var result = await bundleService.SaveCommunityBundleAsync(CurrentUserId, id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGenerated([FromBody] PreviewGeneratedViewModel model)
        {
            if (model == null)
            {
                logger.LogWarning("User {UserId} submitted an empty generated bundle save request.", CurrentUserId);
                return BadRequest(new { message = "Invalid data." });
            }

            List<SystemWordDTO> words;
            try
            {
                words = model.Words;
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "User {UserId} submitted invalid generated bundle words payload.", CurrentUserId);
                return BadRequest(new { message = "Invalid words data." });
            }

            var wordIds = words.Select(w => w.Id).Where(id => id > 0).Distinct().ToList();
            if (!wordIds.Any())
            {
                logger.LogWarning("User {UserId} attempted to save a generated bundle without words.", CurrentUserId);
                return BadRequest(new { message = "Generated bundle has no words to save." });
            }

            var dto = new SaveGeneratedBundleDTO
            {
                Title = model.Title?.Trim() ?? string.Empty,
                Description = model.Description?.Trim(),
                WordIds = wordIds,
                DifficultyLevels = words
                    .Where(w => !string.IsNullOrWhiteSpace(w.DifficultyLevel))
                    .Select(w => w.DifficultyLevel!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                CategoryNames = words
                    .Select(w => w.CategoryName?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            };

            var result = await bundleService.SaveGeneratedBundleAsync(CurrentUserId, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            logger.LogInformation("User {UserId} saved generated bundle '{Title}'.", CurrentUserId, dto.Title);
            return Ok(new { message = "Collection saved.", redirectUrl = Url?.Action("Index", "Bundle") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var result = await bundleService.SubmitBundleForReviewAsync(CurrentUserId, id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Bundle sent for moderation." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await bundleService.DeleteBundleAsync(CurrentUserId, id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Bundle deleted.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Generator()
        {
            var categories = await bundleService.GetAllCategoriesAsync();
            var viewModel = new GeneratorViewModel
            {
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] GenerateBundleDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
            {
                logger.LogWarning("Invalid temporary bundle generation request received.");
                return BadRequest(new { message = "Invalid data." });
            }

            logger.LogInformation("Processing temporary bundle generation request: '{Title}'", dto.Title);

            var result = await bundleService.GenerateBundleAsync(CurrentUserId, dto);
            if (!result.IsSuccess)
            {
                logger.LogWarning("Failed to generate temporary bundle: {Error}", result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            logger.LogInformation("Temporary bundle '{Title}' successfully generated ({WordCount} words).", dto.Title, result.Value!.Words.Count);
            return Ok(result.Value);
        }

        [HttpPost]
        public IActionResult PreviewGenerated([FromForm] PreviewGeneratedViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.WordsJson))
            {
                model.WordsJson = "[]";
            }

            return View(model);
        }

        private async Task ReloadCreateViewModelData(CreateBundleViewModel model)
        {
            var categories = await bundleService.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }

        private async Task ReloadEditViewModelData(EditBundleViewModel model)
        {
            var categories = await bundleService.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }
    }
}