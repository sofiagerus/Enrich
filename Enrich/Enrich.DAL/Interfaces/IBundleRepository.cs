using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IBundleRepository
    {
        Task<(IEnumerable<Bundle> Items, int Total)> GetSystemBundlesPageAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize);

        Task<Bundle?> GetBundleAsync(int bundleId);

        Task<Bundle> CreateBundleAsync(Bundle bundle);
    }
}
