using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;

namespace Enrich.BLL.Interfaces
{
    public interface IBundleService
    {
        Task<Result> CreateBundleAsync(string userId, CreateBundleDTO dto);

        Task<BundleDTO?> GetBundleByIdAsync(int bundleId);

        Task<IEnumerable<BundleDTO>> GetUserBundlesAsync(string userId);

        Task<PagedResult<BundleDTO>> GetUserBundlesPageAsync(
            string userId,
            string? searchTerm,
            string? categoryFilter = null,
            string? difficultyLevel = null,
            int? minWordCount = null,
            int? maxWordCount = null,
            int page = 1,
            int pageSize = 6);

        Task<Result> UpdateBundleAsync(string userId, int bundleId, CreateBundleDTO dto);

        Task<Result> DeleteBundleAsync(string userId, int bundleId);

        Task<Result> AddWordsToBundleAsync(string userId, int bundleId, IEnumerable<int> wordIds);

        Task<Result> RemoveWordsFromBundleAsync(string userId, int bundleId, IEnumerable<int> wordIds);

        Task<PagedResult<SystemBundleDTO>> GetSystemBundlesAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize);

        Task<PagedResult<SystemBundleDTO>> GetCommunityBundlesAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize);

        Task<PagedResult<SystemBundleDTO>> GetPendingBundlesAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize);

        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<Result> SaveSystemBundleAsync(string userId, int bundleId);

        Task<Result> SaveCommunityBundleAsync(string userId, int bundleId);

        Task<Result> SaveGeneratedBundleAsync(string userId, SaveGeneratedBundleDTO dto);

        Task<Result> SubmitBundleForReviewAsync(string userId, int bundleId);

        Task<Result> ReviewBundleAsync(int bundleId, bool approve);

        Task<BundleDTO?> GetBundleWithWordsAsync(int bundleId);

        Task<Result<GeneratedBundleResultDTO>> GenerateBundleAsync(string userId, GenerateBundleDTO dto);

        Task<Result> CreateSystemBundleAsync(CreateBundleDTO dto);

        Task<Result> UpdateSystemBundleAsync(int bundleId, CreateBundleDTO dto);

        Task<Result> UpdateCommunityBundleAsync(int bundleId, CreateBundleDTO dto, BundleStatus newStatus);
      
        Task<Result> DeleteSystemBundleAsync(int bundleId);

        Task<Result> DeleteCommunityBundleAsync(int bundleId);
    }
}