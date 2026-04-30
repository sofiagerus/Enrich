using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enrich.BLL.Services
{
    public class BundleService(
        IBundleRepository bundleRepository,
        IWordRepository wordRepository,
        ICategoryRepository categoryRepository,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings,
        IOptions<PaginationSettings> paginationOptions,
        ILogger<BundleService> logger) : IBundleService
    {
        private readonly PaginationSettings _pagination = paginationOptions.Value;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;

        public async Task<Result> CreateBundleAsync(string userId, CreateBundleDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                logger.LogWarning("Attempt to create a bundle with an empty title by user {UserId}.", userId);
                return "Bundle title cannot be empty.";
            }

            var trimmedTitle = dto.Title.Trim();
            var titleLower = trimmedTitle.ToLowerInvariant();

            var duplicateExists = await bundleRepository.BundleTitleExistsForUserAsync(userId, titleLower);

            if (duplicateExists)
            {
                logger.LogWarning(
                    "User {UserId} attempted to create a duplicate bundle: '{Title}'.",
                    userId,
                    dto.Title);

                return $"You already have a bundle named '{dto.Title}'.";
            }

            var now = DateTime.UtcNow;

            var bundle = new Bundle
            {
                Title = trimmedTitle,
                Description = dto.Description?.Trim(),
                DifficultyLevels = dto.DifficultyLevels ?? [],
                ImageUrl = dto.ImageUrl,
                Status = BundleStatus.Draft,
                IsSystem = false,
                IsPublic = false,
                OwnerId = userId,
                CreatedAt = now,
                UpdatedAt = now,
                Words = new List<Word>(),
                Categories = new List<Category>(),
                Tags = new List<Tag>()
            };

            try
            {
                var createdBundle = await bundleRepository.CreateBundleAsync(bundle);

                if (dto.WordIds != null && dto.WordIds.Any())
                {
                    await bundleRepository.AddWordsToBundleAsync(createdBundle.Id, dto.WordIds);
                }

                if (dto.CategoryIds != null && dto.CategoryIds.Any())
                {
                    await bundleRepository.AddCategoriesToBundleAsync(createdBundle.Id, dto.CategoryIds);
                }

                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    await bundleRepository.AddTagsToBundleAsync(createdBundle.Id, dto.TagIds);
                }

                logger.LogInformation(
                    "User {UserId} successfully created a new bundle '{Title}' (ID: {BundleId}) with status {Status}.",
                    userId,
                    createdBundle.Title,
                    createdBundle.Id,
                    createdBundle.Status);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error creating bundle '{Title}' by user {UserId}.",
                    dto.Title,
                    userId);

                return "An error occurred while creating the bundle. Please try again.";
            }
        }

        public async Task<BundleDTO?> GetBundleByIdAsync(int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdWithDetailsAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning("Attempting to get a non-existent bundle with ID: {BundleId}.", bundleId);
                return null;
            }

            return MapBundleToDTO(bundle);
        }

        public async Task<IEnumerable<BundleDTO>> GetUserBundlesAsync(string userId)
        {
            logger.LogInformation("Retrieving bundle list for user {UserId}.", userId);

            var bundles = await bundleRepository.GetUserBundlesAsync(userId);

            var bundleDTOs = bundles.Select(MapBundleToDTO).ToList();

            logger.LogInformation(
                "Successfully retrieved {Count} bundles for user {UserId}.",
                bundleDTOs.Count,
                userId);

            return bundleDTOs;
        }

        public async Task<PagedResult<BundleDTO>> GetUserBundlesPageAsync(
            string userId,
            string? searchTerm,
            string? categoryFilter = null,
            string? difficultyLevel = null,
            int? minWordCount = null,
            int? maxWordCount = null,
            int page = 1,
            int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = _pagination.DefaultUserBundlesPageSize;
            }

            logger.LogInformation(
                "Fetching page {Page} of bundles for user {UserId} with search '{SearchTerm}', category: '{Category}', level: '{Level}'.",
                page,
                userId,
                searchTerm ?? "(none)",
                categoryFilter ?? "(none)",
                difficultyLevel ?? "(none)");

            var (bundles, total) = await bundleRepository.GetUserBundlesPageAsync(
                userId,
                searchTerm,
                categoryFilter,
                difficultyLevel,
                minWordCount,
                maxWordCount,
                page,
                pageSize);

            var bundleDTOs = bundles.Select(MapBundleToDTO).ToList();

            var result = new PagedResult<BundleDTO>
            {
                Items = bundleDTOs,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            logger.LogInformation(
                "Successfully retrieved {Count} bundles (total {Total}) for user {UserId}.",
                bundleDTOs.Count,
                total,
                userId);

            return result;
        }

        public async Task<PagedResult<SystemBundleDTO>> GetSystemBundlesAsync(
            string? searchTerm,
            string? category,
            string? difficultyLevel,
            int? minWordCount,
            int? maxWordCount,
            int page,
            int pageSize)
        {
            logger.LogInformation("Fetching page of system bundles: page={Page}, size={PageSize}", page, pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = _pagination.DefaultSystemBundlesPageSize;
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

        public async Task<PagedResult<SystemBundleDTO>> GetCommunityBundlesAsync(
             string? searchTerm,
             string? category,
             string? difficultyLevel,
             int? minWordCount,
             int? maxWordCount,
             int page,
             int pageSize)
        {
            logger.LogInformation("Retrieving community bundles page: page={Page}, size={PageSize}", page, pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = _pagination.DefaultSystemBundlesPageSize;
            }

            var (bundles, total) = await bundleRepository.GetCommunityBundlesPageAsync(
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

        public async Task<PagedResult<SystemBundleDTO>> GetPendingBundlesAsync(
             string? searchTerm,
             string? category,
             string? difficultyLevel,
             int? minWordCount,
             int? maxWordCount,
             int page,
             int pageSize)
        {
            logger.LogInformation("Retrieving pending review bundles page: page={Page}, size={PageSize}", page, pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = _pagination.DefaultSystemBundlesPageSize;
            }

            var (bundles, total) = await bundleRepository.GetPendingBundlesPageAsync(
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
            return await cache.GetOrCreateAsync(CacheKeys.AllCategories, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.CategoriesCacheDurationMinutes);
                return await categoryRepository.GetAllCategoriesAsync();
            }) ?? [];
        }

        public async Task<Result> UpdateBundleAsync(string userId, int bundleId, CreateBundleDTO dto)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "User {UserId} attempted to update non-existent bundle {BundleId}.",
                    userId,
                    bundleId);

                return "Bundle not found.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "User {UserId} attempted to update someone else's bundle {BundleId} (owner: {OwnerId}).",
                    userId,
                    bundleId,
                    bundle.OwnerId);

                return "You do not have permission to update this bundle.";
            }

            if (bundle.Status is not BundleStatus.Draft and not BundleStatus.Rejected)
            {
                logger.LogWarning(
                    "User {UserId} attempted to update bundle {BundleId} which is in status {Status}.",
                    userId,
                    bundleId,
                    bundle.Status);

                return "You cannot edit a bundle that is under review or published.";
            }

            if (!bundle.Title.Equals(dto.Title, StringComparison.OrdinalIgnoreCase))
            {
                var titleLower = dto.Title.Trim().ToLowerInvariant();
                var duplicateExists = await bundleRepository.BundleTitleExistsForUserAsync(userId, titleLower);

                if (duplicateExists)
                {
                    logger.LogWarning(
                        "User {UserId} attempted to update bundle {BundleId} with a duplicate title '{Title}'.",
                        userId,
                        bundleId,
                        dto.Title);

                    return $"You already have a bundle named '{dto.Title}'.";
                }

                bundle.Title = dto.Title.Trim();
            }

            bundle.Description = dto.Description?.Trim();
            bundle.DifficultyLevels = dto.DifficultyLevels ?? [];
            bundle.ImageUrl = dto.ImageUrl;
            bundle.UpdatedAt = DateTime.UtcNow;

            try
            {
                await bundleRepository.UpdateBundleAsync(bundle);
                await bundleRepository.SyncBundleRelationsAsync(bundle.Id, dto.WordIds, dto.CategoryIds);

                logger.LogInformation(
                    "Bundle {BundleId} of user {UserId} successfully updated.",
                    bundleId,
                    userId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error updating bundle {BundleId} by user {UserId}.",
                    bundleId,
                    userId);

                return "An error occurred while updating the bundle. Please try again.";
            }
        }

        public async Task<Result> DeleteBundleAsync(string userId, int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "User {UserId} attempted to delete non-existent bundle {BundleId}.",
                    userId,
                    bundleId);

                return "Bundle not found.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "User {UserId} attempted to delete someone else's bundle {BundleId} (owner: {OwnerId}).",
                    userId,
                    bundleId,
                    bundle.OwnerId);

                return "You do not have permission to delete this bundle.";
            }

            if (bundle.Status is not BundleStatus.Draft and not BundleStatus.Rejected)
            {
                logger.LogWarning(
                    "User {UserId} attempted to delete bundle {BundleId} which is in status {Status}.",
                    userId,
                    bundleId,
                    bundle.Status);

                return "You cannot delete a bundle that is under review or published.";
            }

            try
            {
                await bundleRepository.DeleteBundleAsync(bundleId);

                logger.LogInformation(
                    "Bundle {BundleId} for user {UserId} successfully deleted.",
                    bundleId,
                    userId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error deleting bundle {BundleId} by user {UserId}.",
                    bundleId,
                    userId);

                return "An error occurred while deleting the bundle. Please try again.";
            }
        }

        public async Task<Result> AddWordsToBundleAsync(string userId, int bundleId, IEnumerable<int> wordIds)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "User {UserId} attempted to add words to non-existent bundle {BundleId}.",
                    userId,
                    bundleId);

                return "Bundle not found.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "User {UserId} attempted to add words to someone else's bundle {BundleId}.",
                    userId,
                    bundleId);

                return "You do not have permission to modify this bundle.";
            }

            try
            {
                await bundleRepository.AddWordsToBundleAsync(bundleId, wordIds);

                logger.LogInformation(
                    "User {UserId} successfully added {Count} words to bundle {BundleId}.",
                    userId,
                    wordIds.Count(),
                    bundleId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error adding words to bundle {BundleId} by user {UserId}.",
                    bundleId,
                    userId);

                return "An error occurred while adding words. Please try again.";
            }
        }

        public async Task<Result> RemoveWordsFromBundleAsync(string userId, int bundleId, IEnumerable<int> wordIds)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "User {UserId} attempted to remove words from non-existent bundle {BundleId}.",
                    userId,
                    bundleId);

                return "Bundle not found.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "User {UserId} attempted to remove words from someone else's bundle {BundleId}.",
                    userId,
                    bundleId);

                return "You do not have permission to modify this bundle.";
            }

            try
            {
                await bundleRepository.RemoveWordsFromBundleAsync(bundleId, wordIds);

                logger.LogInformation(
                    "User {UserId} successfully removed {Count} words from bundle {BundleId}.",
                    userId,
                    wordIds.Count(),
                    bundleId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error removing words from bundle {BundleId} by user {UserId}.",
                    bundleId,
                    userId);

                return "An error occurred while removing words. Please try again.";
            }
        }

        private static BundleDTO MapBundleToDTO(Bundle bundle)
        {
            return new BundleDTO
            {
                Id = bundle.Id,
                Title = bundle.Title,
                Description = bundle.Description,
                DifficultyLevels = bundle.DifficultyLevels,
                ImageUrl = bundle.ImageUrl,
                Status = bundle.Status.ToString(),
                IsPublic = bundle.IsPublic,
                IsSystem = bundle.IsSystem,
                OwnerId = bundle.OwnerId,
                CreatedAt = bundle.CreatedAt,
                UpdatedAt = bundle.UpdatedAt,
                WordCount = bundle.Words?.Count ?? 0,
                CategoryCount = bundle.Categories?.Count ?? 0,
                TagCount = bundle.Tags?.Count ?? 0,
                Categories = bundle.Categories?.Select(c => c.Name).ToList() ?? [],
                CategoryIds = bundle.Categories?.Select(c => c.Id).ToList() ?? [],
                WordIds = bundle.Words?.Select(w => w.Id).ToList() ?? [],
                Words = bundle.Words?.Select(w => new BundleWordDTO
                {
                    Id = w.Id,
                    Term = w.Term,
                    Translation = w.Translation,
                    PartOfSpeech = w.PartOfSpeech,
                    Example = w.Example
                }).ToList() ?? []
            };
        }

        public async Task<BundleDTO?> GetBundleWithWordsAsync(int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdWithDetailsAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning("Bundle with ID {BundleId} not found.", bundleId);
                return null;
            }

            return MapBundleToDTO(bundle);
        }

        public async Task<Result> SaveSystemBundleAsync(string userId, int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null || !bundle.IsSystem)
            {
                logger.LogWarning("User {UserId} attempted to save non-existent or non-system bundle {BundleId}.", userId, bundleId);
                return "Collection not found or is not a system collection.";
            }

            var alreadySaved = await bundleRepository.UserHasBundleAsync(userId, bundleId);
            if (alreadySaved)
            {
                return "You have already saved this collection.";
            }

            var userBundle = new UserBundle
            {
                UserId = userId,
                BundleId = bundleId,
                SavedAt = DateTime.UtcNow
            };

            try
            {
                await bundleRepository.SaveUserBundleAsync(userBundle);
                logger.LogInformation("User {UserId} successfully saved system bundle {BundleId}.", userId, bundleId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving system bundle {BundleId} by user {UserId}.", bundleId, userId);
                return "An error occurred while saving the collection.";
            }
        }

        public async Task<Result> SaveCommunityBundleAsync(string userId, int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null || bundle.IsSystem || bundle.Status != BundleStatus.Published)
            {
                logger.LogWarning("User {UserId} attempted to save non-existent or invalid community bundle {BundleId}.", userId, bundleId);
                return "Collection not found or is not a valid community collection.";
            }

            var alreadySaved = await bundleRepository.UserHasBundleAsync(userId, bundleId);
            if (alreadySaved)
            {
                return "You have already saved this collection.";
            }

            var userBundle = new UserBundle
            {
                UserId = userId,
                BundleId = bundleId,
                SavedAt = DateTime.UtcNow
            };

            try
            {
                await bundleRepository.SaveUserBundleAsync(userBundle);
                logger.LogInformation("User {UserId} successfully saved community bundle {BundleId}.", userId, bundleId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving community bundle {BundleId} by user {UserId}.", bundleId, userId);
                return "An error occurred while saving the collection.";
            }
        }

        public async Task<Result> SaveGeneratedBundleAsync(string userId, SaveGeneratedBundleDTO dto)
        {
            if (dto == null)
            {
                logger.LogWarning("User {UserId} attempted to save a generated bundle with empty payload.", userId);
                return "Invalid bundle data.";
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                logger.LogWarning("User {UserId} attempted to save a generated bundle with an empty title.", userId);
                return "Bundle title cannot be empty.";
            }

            var wordIds = dto.WordIds?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            if (!wordIds.Any())
            {
                logger.LogWarning("User {UserId} attempted to save a generated bundle '{Title}' with no words.", userId, dto.Title);
                return "Generated bundle has no words to save.";
            }

            var titleLower = dto.Title.Trim().ToLowerInvariant();
            var duplicateExists = await bundleRepository.BundleTitleExistsForUserAsync(userId, titleLower);
            if (duplicateExists)
            {
                logger.LogWarning("User {UserId} attempted to save a duplicate generated bundle '{Title}'.", userId, dto.Title);
                return $"You already have a bundle named '{dto.Title}'.";
            }

            var difficultyLevels = dto.DifficultyLevels?
                .Where(level => !string.IsNullOrWhiteSpace(level))
                .Select(level => level.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];

            var now = DateTime.UtcNow;
            var bundle = new Bundle
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                DifficultyLevels = difficultyLevels,
                Status = BundleStatus.Draft,
                IsSystem = false,
                IsPublic = false,
                OwnerId = userId,
                CreatedAt = now,
                UpdatedAt = now,
                Words = new List<Word>(),
                Categories = new List<Category>(),
                Tags = new List<Tag>()
            };

            try
            {
                var createdBundle = await bundleRepository.CreateBundleAsync(bundle);

                await bundleRepository.AddWordsToBundleAsync(createdBundle.Id, wordIds);

                var categoryNames = dto.CategoryNames?
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (categoryNames.Any())
                {
                    var categoryIds = new List<int>();
                    foreach (var name in categoryNames)
                    {
                        var category = await categoryRepository.GetCategoryByNameAsync(name);
                        if (category != null)
                        {
                            categoryIds.Add(category.Id);
                        }
                    }

                    if (categoryIds.Any())
                    {
                        await bundleRepository.AddCategoriesToBundleAsync(createdBundle.Id, categoryIds.Distinct());
                    }
                }

                logger.LogInformation(
                    "User {UserId} saved generated bundle '{Title}' (ID: {BundleId}) with {WordCount} words.",
                    userId,
                    createdBundle.Title,
                    createdBundle.Id,
                    wordIds.Count);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving generated bundle '{Title}' by user {UserId}.", dto.Title, userId);
                return "An error occurred while saving the generated bundle.";
            }
        }

        public async Task<Result> SubmitBundleForReviewAsync(string userId, int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning("User {UserId} attempted to submit non-existent bundle {BundleId} for review.", userId, bundleId);
                return "Bundle not found.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning("User {UserId} attempted to submit someone else's bundle {BundleId} for review.", userId, bundleId);
                return "You do not have permission to submit this bundle.";
            }

            if (bundle.Status is not BundleStatus.Draft and not BundleStatus.Rejected)
            {
                logger.LogWarning(
                    "User {UserId} attempted to submit bundle {BundleId} for review but it is already in status {Status}.",
                    userId,
                    bundleId,
                    bundle.Status);

                return "This bundle is already submitted or published.";
            }

            bundle.Status = BundleStatus.PendingReview;
            bundle.UpdatedAt = DateTime.UtcNow;

            try
            {
                await bundleRepository.UpdateBundleAsync(bundle);
                logger.LogInformation("User {UserId} successfully submitted bundle {BundleId} for review.", userId, bundleId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error submitting bundle {BundleId} for review by user {UserId}.", bundleId, userId);
                return "An error occurred while submitting the bundle.";
            }
        }

        public async Task<Result> ReviewBundleAsync(int bundleId, bool approve)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning("Attempted to review non-existent bundle {BundleId}.", bundleId);
                return "Bundle not found.";
            }

            if (bundle.Status != BundleStatus.PendingReview)
            {
                logger.LogWarning("Attempted to review bundle {BundleId} which is in status {Status}.", bundleId, bundle.Status);
                return "Bundle is not pending review.";
            }

            bundle.Status = approve ? BundleStatus.Published : BundleStatus.Rejected;
            bundle.UpdatedAt = DateTime.UtcNow;
            bundle.ReviewedAt = DateTime.UtcNow;

            try
            {
                await bundleRepository.UpdateBundleAsync(bundle);
                logger.LogInformation("Bundle {BundleId} was successfully {Action}.", bundleId, approve ? "published" : "rejected");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reviewing bundle {BundleId}.", bundleId);
                return "An error occurred while reviewing the bundle.";
            }
        }

        public async Task<Result<GeneratedBundleResultDTO>> GenerateBundleAsync(string userId, GenerateBundleDTO dto)
        {
            logger.LogInformation("User {UserId} started temporary bundle generation: '{Title}'", userId, dto.Title);

            var allFoundWords = new List<Word>();
            var allFoundWordIds = new HashSet<int>();

            foreach (var rule in dto.Rules)
            {
                var words = await wordRepository.GetRandomWordsByCriteriaAsync(
                    rule.CategoryId,
                    rule.PartOfSpeech,
                    rule.MinDifficulty,
                    rule.MaxDifficulty,
                    rule.WordCount);

                foreach (var word in words)
                {
                    if (allFoundWordIds.Add(word.Id))
                    {
                        allFoundWords.Add(word);
                    }
                }
            }

            if (!allFoundWords.Any())
            {
                logger.LogWarning("No words found for bundle '{Title}' requested by user {UserId}.", dto.Title, userId);
                return "No words found matching the specified criteria.";
            }

            var result = new GeneratedBundleResultDTO
            {
                Title = dto.Title,
                Description = dto.Description,
                Words = allFoundWords.Select(w => new SystemWordDTO
                {
                    Id = w.Id,
                    Term = w.Term,
                    Translation = w.Translation,
                    Transcription = w.Transcription,
                    Meaning = w.Meaning,
                    PartOfSpeech = w.PartOfSpeech,
                    Example = w.Example,
                    DifficultyLevel = w.DifficultyLevel,
                    CategoryName = w.Categories.FirstOrDefault()?.Name ?? "General"
                }).ToList()
            };

            logger.LogInformation("Temporary bundle '{Title}' successfully generated ({WordCount} words).", dto.Title, result.Words.Count);

            return result;
        }

        public async Task<Result> CreateSystemBundleAsync(CreateBundleDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                logger.LogWarning("Attempt to create a system bundle with an empty title.");
                return "Bundle title cannot be empty.";
            }

            var now = DateTime.UtcNow;

            var bundle = new Bundle
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                DifficultyLevels = dto.DifficultyLevels ?? [],
                ImageUrl = dto.ImageUrl,
                Status = BundleStatus.Published, // Системні бандли відразу опубліковані
                IsSystem = true,                 // Головна відмінність!
                IsPublic = true,
                OwnerId = "SYSTEM",              // Власник - система
                CreatedAt = now,
                UpdatedAt = now,
                Words = new List<Word>(),
                Categories = new List<Category>(),
                Tags = new List<Tag>()
            };

            try
            {
                var createdBundle = await bundleRepository.CreateBundleAsync(bundle);

                if (dto.WordIds != null && dto.WordIds.Any())
                {
                    await bundleRepository.AddWordsToBundleAsync(createdBundle.Id, dto.WordIds);
                }

                if (dto.CategoryIds != null && dto.CategoryIds.Any())
                {
                    await bundleRepository.AddCategoriesToBundleAsync(createdBundle.Id, dto.CategoryIds);
                }

                logger.LogInformation("System bundle '{Title}' (ID: {BundleId}) successfully created.", createdBundle.Title, createdBundle.Id);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating system bundle '{Title}'.", dto.Title);
                return "An error occurred while creating the system bundle.";
            }
        }

        public async Task<Result> UpdateSystemBundleAsync(int bundleId, CreateBundleDTO dto)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null || !bundle.IsSystem)
            {
                logger.LogWarning("Attempted to update non-existent or non-system bundle {BundleId}.", bundleId);
                return "System bundle not found.";
            }

            bundle.Title = dto.Title.Trim();
            bundle.Description = dto.Description?.Trim();
            bundle.DifficultyLevels = dto.DifficultyLevels ?? [];
            bundle.ImageUrl = dto.ImageUrl;
            bundle.UpdatedAt = DateTime.UtcNow;

            try
            {
                await bundleRepository.UpdateBundleAsync(bundle);
                await bundleRepository.SyncBundleRelationsAsync(bundle.Id, dto.WordIds, dto.CategoryIds);

                logger.LogInformation("System bundle {BundleId} successfully updated.", bundleId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating system bundle {BundleId}.", bundleId);
                return "An error occurred while updating the system bundle.";
            }
        }

        public async Task<Result> DeleteSystemBundleAsync(int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null || !bundle.IsSystem)
            {
                logger.LogWarning("Attempted to delete non-existent or non-system bundle {BundleId}.", bundleId);
                return "System bundle not found.";
            }

            if (bundle.Status != BundleStatus.Published)
            {
                logger.LogWarning(
                    "Attempted to delete system bundle {BundleId} which is in status {Status}.",
                    bundleId,
                    bundle.Status);

                return "Only published system bundles can be deleted.";
            }

            try
            {
                await bundleRepository.DeleteBundleAsync(bundleId);

                logger.LogInformation("System bundle {BundleId} ('{Title}') successfully deleted.", bundleId, bundle.Title);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting system bundle {BundleId}.", bundleId);
                return "An error occurred while deleting the system bundle.";
            }
        }

        public async Task<Result> DeleteCommunityBundleAsync(int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning("Attempted to delete non-existent bundle {BundleId}.", bundleId);
                return "Collection not found.";
            }

            if (bundle.IsSystem || bundle.Status != BundleStatus.Published)
            {
                logger.LogWarning(
                    "Attempted to delete invalid community bundle {BundleId} (IsSystem: {IsSystem}, Status: {Status}).",
                    bundleId,
                    bundle.IsSystem,
                    bundle.Status);

                return "Invalid community bundle.";
            }

            try
            {
                await bundleRepository.DeleteBundleAsync(bundleId);
                logger.LogInformation("Community bundle {BundleId} ('{Title}') successfully deleted.", bundleId, bundle.Title);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting community bundle {BundleId}.", bundleId);
                return "An error occurred while deleting the community bundle.";
            }
        }
    }
}