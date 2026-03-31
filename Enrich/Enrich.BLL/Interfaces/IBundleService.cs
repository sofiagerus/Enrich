using Enrich.BLL.DTOs;
using Enrich.DAL.Entities;

namespace Enrich.BLL.Interfaces
{
    public interface IBundleService
    {
        Task<PagedResult<SystemBundleDTO>> GetSystemBundlesAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize);

        Task<IEnumerable<Category>> GetAllCategoriesAsync();
    }
}
