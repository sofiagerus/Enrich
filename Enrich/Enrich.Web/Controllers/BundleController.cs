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
        ICategoryRepository categoryRepository,
        IWordRepository wordRepository,
        IStudySessionService studySessionService,
        IOptions<PaginationSettings> paginationOptions) : BaseController
    {
        public ILogger<BundleController> Logger { get; } = logger;

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
                Categories = categories,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
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
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка створення.");
                await ReloadCreateViewModelData(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"Бандл '{model.Title}' успішно створено!";
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
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Помилка оновлення.");
                await ReloadEditViewModelData(model);
                return View(model);
            }

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
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Бандл видалено.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Study(int bundleId)
        {
            var result = await studySessionService.StartStudySessionAsync(CurrentUserId, bundleId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Не вдалося розпочати навчання.";
                return RedirectToAction("Index");
            }

            var sessionDto = result.Value!;
            if (sessionDto.TotalCards == 0)
            {
                TempData["ErrorMessage"] = "Бандл порожній.";
                return RedirectToAction("Index");
            }

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
            if (dto == null)
            {
                return BadRequest(new { message = "Дані не надано." });
            }

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
            if (request == null)
            {
                return BadRequest();
            }

            var result = await studySessionService.FinishStudySessionAsync(CurrentUserId, request.SessionId);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { redirectUrl = Url.Action("Index", "Bundle") });
        }

        private async Task ReloadCreateViewModelData(CreateBundleViewModel model)
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }

        private async Task ReloadEditViewModelData(EditBundleViewModel model)
        {
            var categories = await categoryRepository.GetAllCategoriesAsync();
            var words = await wordRepository.GetAllWordsAsync();
            model.Categories = categories.Select(c => (c.Id, c.Name)).ToList();
            model.Words = words.Select(w => new WordItemViewModel { Id = w.Id, Term = w.Term, Translation = w.Translation }).ToList();
            model.AvailableLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
        }
    }
}