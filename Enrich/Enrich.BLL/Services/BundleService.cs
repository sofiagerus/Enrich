using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.DAL.Entities.Enums;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class BundleService(
        IBundleRepository bundleRepository,
        ICategoryRepository categoryRepository,
        ILogger<BundleService> logger) : IBundleService
    {
        public async Task<Result> CreateBundleAsync(string userId, CreateBundleDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                logger.LogWarning("Спроба створити бандл з порожною назвою користувачем {UserId}.", userId);
                return "Назва бандлу не може бути порожною.";
            }

            var titleLower = dto.Title.Trim().ToLowerInvariant();

            var duplicateExists = await bundleRepository.BundleTitleExistsForUserAsync(userId, titleLower);

            if (duplicateExists)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував створити дублікат бандлу: '{Title}'.",
                    userId,
                    dto.Title);

                return $"Ви вже маєте бандл з назвою '{dto.Title}'.";
            }

            var now = DateTime.UtcNow;

            var bundle = new Bundle
            {
                Title = dto.Title.Trim(),
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
                    "Користувач {UserId} успішно створив новий бандл '{Title}' (ID: {BundleId}) зі статусом {Status}.",
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
                    "Помилка при створенні бандлу '{Title}' користувачем {UserId}.",
                    dto.Title,
                    userId);

                return "При створенні бандлу сталася помилка. Спробуйте ще раз.";
            }
        }

        public async Task<BundleDTO?> GetBundleByIdAsync(int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdWithDetailsAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning("Спроба отримати неіснуючий бандл з ID: {BundleId}.", bundleId);
                return null;
            }

            return MapBundleToDTO(bundle);
        }

        public async Task<IEnumerable<BundleDTO>> GetUserBundlesAsync(string userId)
        {
            logger.LogInformation("Отримання списку бандлів користувача {UserId}.", userId);

            var bundles = await bundleRepository.GetUserBundlesAsync(userId);

            var bundleDTOs = bundles.Select(MapBundleToDTO).ToList();

            logger.LogInformation(
                "Успішно отримано {Count} бандлів для користувача {UserId}.",
                bundleDTOs.Count,
                userId);

            return bundleDTOs;
        }

        public async Task<PagedResult<BundleDTO>> GetUserBundlesPageAsync(
            string userId,
            string? searchTerm,
            int page,
            int pageSize)
        {
            logger.LogInformation(
                "Отримання сторінки {Page} бандлів користувача {UserId} з пошуком '{SearchTerm}'.",
                page,
                userId,
                searchTerm ?? "(немає)");

            var (bundles, total) = await bundleRepository.GetUserBundlesPageAsync(userId, searchTerm, page, pageSize);

            var bundleDTOs = bundles.Select(MapBundleToDTO).ToList();

            var result = new PagedResult<BundleDTO>
            {
                Items = bundleDTOs,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            logger.LogInformation(
                "Успішно отримано {Count} бандлів (всього {Total}) для користувача {UserId}.",
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

        public async Task<Result> UpdateBundleAsync(string userId, int bundleId, CreateBundleDTO dto)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував оновити неіснуючий бандл {BundleId}.",
                    userId,
                    bundleId);

                return "Бандл не знайдено.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував оновити чужий бандл {BundleId} (власник: {OwnerId}).",
                    userId,
                    bundleId,
                    bundle.OwnerId);

                return "Ви не маєте права оновлювати цей бандл.";
            }

            if (!bundle.Title.Equals(dto.Title, StringComparison.OrdinalIgnoreCase))
            {
                var titleLower = dto.Title.Trim().ToLowerInvariant();
                var duplicateExists = await bundleRepository.BundleTitleExistsForUserAsync(userId, titleLower);

                if (duplicateExists)
                {
                    logger.LogWarning(
                        "Користувач {UserId} спробував оновити бандл {BundleId} дублікатною назвою '{Title}'.",
                        userId,
                        bundleId,
                        dto.Title);

                    return $"Ви вже маєте бандл з назвою '{dto.Title}'.";
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

                logger.LogInformation(
                    "Бандл {BundleId} користувача {UserId} успішно оновлено.",
                    bundleId,
                    userId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при оновленні бандлу {BundleId} користувачем {UserId}.",
                    bundleId,
                    userId);

                return "При оновленні бандлу сталася помилка. Спробуйте ще раз.";
            }
        }

        public async Task<Result> DeleteBundleAsync(string userId, int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував видалити неіснуючий бандл {BundleId}.",
                    userId,
                    bundleId);

                return "Бандл не знайдено.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував видалити чужий бандл {BundleId} (власник: {OwnerId}).",
                    userId,
                    bundleId,
                    bundle.OwnerId);

                return "Ви не маєте права видаляти цей бандл.";
            }

            try
            {
                await bundleRepository.DeleteBundleAsync(bundleId);

                logger.LogInformation(
                    "Бандл {BundleId} користувача {UserId} успішно видалено.",
                    bundleId,
                    userId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при видаленні бандлу {BundleId} користувачем {UserId}.",
                    bundleId,
                    userId);

                return "При видаленні бандлу сталася помилка. Спробуйте ще раз.";
            }
        }

        public async Task<Result> AddWordsToBundleAsync(string userId, int bundleId, IEnumerable<int> wordIds)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував додати слова до неіснуючого бандлу {BundleId}.",
                    userId,
                    bundleId);

                return "Бандл не знайдено.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував додати слова до чужого бандлу {BundleId}.",
                    userId,
                    bundleId);

                return "Ви не маєте права змінювати цей бандл.";
            }

            try
            {
                await bundleRepository.AddWordsToBundleAsync(bundleId, wordIds);

                logger.LogInformation(
                    "Користувач {UserId} успішно додав {Count} слів до бандлу {BundleId}.",
                    userId,
                    wordIds.Count(),
                    bundleId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при додаванні слів до бандлу {BundleId} користувачем {UserId}.",
                    bundleId,
                    userId);

                return "При додаванні слів сталася помилка. Спробуйте ще раз.";
            }
        }

        public async Task<Result> RemoveWordsFromBundleAsync(string userId, int bundleId, IEnumerable<int> wordIds)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував видалити слова з неіснуючого бандлу {BundleId}.",
                    userId,
                    bundleId);

                return "Бандл не знайдено.";
            }

            if (bundle.OwnerId != userId)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував видалити слова з чужого бандлу {BundleId}.",
                    userId,
                    bundleId);

                return "Ви не маєте права змінювати цей бандл.";
            }

            try
            {
                await bundleRepository.RemoveWordsFromBundleAsync(bundleId, wordIds);

                logger.LogInformation(
                    "Користувач {UserId} успішно видалив {Count} слів з бандлу {BundleId}.",
                    userId,
                    wordIds.Count(),
                    bundleId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при видаленні слів з бандлу {BundleId} користувачем {UserId}.",
                    bundleId,
                    userId);

                return "При видаленні слів сталася помилка. Спробуйте ще раз.";
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
                OwnerId = bundle.OwnerId,
                CreatedAt = bundle.CreatedAt,
                UpdatedAt = bundle.UpdatedAt,
                WordCount = bundle.Words?.Count ?? 0,
                CategoryCount = bundle.Categories?.Count ?? 0,
                TagCount = bundle.Tags?.Count ?? 0
            };
        }

        public async Task<Result> SaveSystemBundleAsync(string userId, int bundleId)
        {
            var bundle = await bundleRepository.GetBundleByIdAsync(bundleId);

            if (bundle == null || !bundle.IsSystem)
            {
                logger.LogWarning("Користувач {UserId} спробував зберегти неіснуючий або несистемний бандл {BundleId}.", userId, bundleId);
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
                logger.LogInformation("Користувач {UserId} успішно зберіг системний бандл {BundleId}.", userId, bundleId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Помилка при збереженні системного бандлу {BundleId} користувачем {UserId}.", bundleId, userId);
                return "An error occurred while saving the collection.";
            }
        }
    }
}