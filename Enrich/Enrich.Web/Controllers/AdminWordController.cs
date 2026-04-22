using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enrich.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminWordController(
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
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateWordViewModel
            {
                Categories = await wordService.GetAllCategoriesAsync() ?? new List<Category>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await wordService.GetAllCategoriesAsync();
                return View(model);
            }

            var categoryIds = new List<int>();
            if (!string.IsNullOrEmpty(model.NewCategory))
            {
                // Simple category logic or inject something
                var cat = await wordService.GetCategoryByNameAsync(model.NewCategory)
                       ?? await wordService.CreateCategoryAsync(model.NewCategory);
                categoryIds.Add(cat.Id);
            }

            var dto = new CreateSystemWordDTO
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

            var result = await wordService.CreateSystemWordAsync(dto);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "System word created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage!);
            model.Categories = await wordService.GetAllCategoriesAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await wordService.GetSystemWordForEditAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound();
            }

            var word = result.Value!;
            var vm = new EditSystemWordViewModel
            {
                Id = word.Id,
                Term = word.Term,
                Translation = word.Translation,
                Transcription = word.Transcription,
                Meaning = word.Meaning,
                PartOfSpeech = word.PartOfSpeech,
                Example = word.Example,
                DifficultyLevel = word.DifficultyLevel,
                Categories = await wordService.GetAllCategoriesAsync() ?? new List<Category>()
            };

            var cat = word.Categories.FirstOrDefault();
            if (cat != null)
            {
                vm.NewCategory = cat.Name;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditSystemWordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await wordService.GetAllCategoriesAsync();
                return View(model);
            }

            var categoryIds = new List<int>();
            if (!string.IsNullOrEmpty(model.NewCategory))
            {
                var cat = await wordService.GetCategoryByNameAsync(model.NewCategory)
                       ?? await wordService.CreateCategoryAsync(model.NewCategory);
                categoryIds.Add(cat.Id);
            }

            var dto = new UpdateSystemWordDTO
            {
                Id = id,
                Term = model.Term.Trim(),
                Translation = model.Translation?.Trim(),
                Transcription = model.Transcription?.Trim(),
                Meaning = model.Meaning?.Trim(),
                PartOfSpeech = model.PartOfSpeech?.Trim(),
                Example = model.Example?.Trim(),
                DifficultyLevel = model.DifficultyLevel?.Trim(),
                CategoryIds = categoryIds
            };

            var result = await wordService.UpdateSystemWordAsync(id, dto);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "System word updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage!);
            model.Categories = await wordService.GetAllCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await wordService.DeleteSystemWordAsync(id);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "System word deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
