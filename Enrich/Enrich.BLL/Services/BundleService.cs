using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class BundleService(
        IBundleRepository bundleRepository,
        ICategoryRepository categoryRepository,
        ILogger<BundleService> logger) : IBundleService
    {
        public async Task<PagedResult<SystemBundleDTO>> GetSystemBundlesAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize)
        {
            logger.LogInformation("Getting system bundles page: page={Page}, pageSize={PageSize}", page, pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 12;
            }

            var (bundles, total) = await bundleRepository.GetSystemBundlesPageAsync(
                searchTerm,
                category,
                difficultyLevel,
                minWordCount,
                maxWordCount,
                page,
                pageSize);

            var items = bundles.Select(b => new SystemBundleDTO
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                ImageUrl = b.ImageUrl,
                WordCount = b.Words?.Count ?? 0,
                DifficultyLevels = b.DifficultyLevels ?? [],
                Categories = b.Categories?.Select(c => c.Name).ToList() ?? []
            }).ToList();

            return new PagedResult<SystemBundleDTO>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await categoryRepository.GetAllCategoriesAsync();
        }
    }
}
