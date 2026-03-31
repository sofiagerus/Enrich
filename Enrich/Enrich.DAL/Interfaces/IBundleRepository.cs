using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IBundleRepository
    {
        Task<Bundle> CreateBundleAsync(Bundle bundle);

        Task<Bundle?> GetBundleByIdAsync(int bundleId);

        Task<Bundle?> GetBundleByIdWithDetailsAsync(int bundleId);

        Task<IEnumerable<Bundle>> GetUserBundlesAsync(string userId);

        Task<(IEnumerable<Bundle> Items, int Total)> GetUserBundlesPageAsync(string userId, string? searchTerm, int page, int pageSize);

        Task UpdateBundleAsync(Bundle bundle);

        Task DeleteBundleAsync(int bundleId);

        Task<bool> BundleTitleExistsForUserAsync(string userId, string titleLower);

        Task AddWordsToBundleAsync(int bundleId, IEnumerable<int> wordIds);

        Task RemoveWordsFromBundleAsync(int bundleId, IEnumerable<int> wordIds);

        Task AddCategoriesToBundleAsync(int bundleId, IEnumerable<int> categoryIds);

        Task AddTagsToBundleAsync(int bundleId, IEnumerable<int> tagIds);
    }
}
