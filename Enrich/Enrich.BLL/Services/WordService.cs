using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.BLL.Settings;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enrich.BLL.Services
{
    public class WordService(
        IWordRepository wordRepository,
        ICategoryRepository categoryRepository,
        IWordProgressRepository wordProgressRepository,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings,
        IOptions<PaginationSettings> paginationOptions,
        ILogger<WordService> logger) : IWordService
    {
        private readonly PaginationSettings _pagination = paginationOptions.Value;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;

        public async Task<Result> CreatePersonalWordAsync(string userId, CreatePersonalWordDTO dto)
        {
            var termLower = dto.Term.Trim().ToLowerInvariant();

            var duplicateExists = await wordRepository.WordExistsForUserAsync(userId, termLower);

            if (duplicateExists)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував створити дублікат слова: '{Term}'.",
                    userId,
                    dto.Term);

                return $"У вашому особистому словнику вже є слово '{dto.Term}'.";
            }

            var now = DateTime.UtcNow;

            var categories = new List<Category>();
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var fetchedCategories = await categoryRepository.GetCategoriesByIdsAsync(dto.CategoryIds);
                if (fetchedCategories != null)
                {
                    categories = [.. fetchedCategories];
                }
            }

            var word = new Word
            {
                Term = dto.Term.Trim(),
                Translation = dto.Translation?.Trim(),
                Transcription = dto.Transcription?.Trim(),
                Meaning = dto.Meaning?.Trim(),
                PartOfSpeech = dto.PartOfSpeech?.Trim(),
                Example = dto.Example?.Trim(),
                DifficultyLevel = dto.DifficultyLevel?.Trim(),
                IsGlobal = false,
                CreatorId = userId,
                CreatedAt = now,
                UpdatedAt = now,
                Categories = categories
            };

            var userWord = new UserWord
            {
                UserId = userId,
                SavedAt = now,
            };

            var createdWord = await wordRepository.CreatePersonalWordAsync(word, userWord);

            logger.LogInformation(
                "Користувач {UserId} успішно створив нове персональне слово '{Term}' (ID: {WordId}) з {CatCount} категоріями.",
                userId,
                createdWord.Term,
                createdWord.Id,
                categories.Count);

            return true;
        }

        public async Task<IEnumerable<PersonalWordDTO>> GetPersonalWordsAsync(string userId)
        {
            logger.LogInformation("Отримання списку персональних слів для користувача {UserId}", userId);

            var userWords = await wordRepository.GetPersonalWordsWithDetailsAsync(userId);
            var wordIds = userWords.Select(uw => uw.WordId).ToList();

            var progresses = await wordProgressRepository.GetWordProgressesAsync(userId, wordIds);
            var progressDict = progresses.ToDictionary(p => p.WordId, p => p.Points);

            var words = userWords.Select(uw => new PersonalWordDTO
            {
                Id = uw.Word.Id,
                Term = uw.Word.Term,
                Translation = uw.Word.Translation,
                Transcription = uw.Word.Transcription,
                Meaning = uw.Word.Meaning,
                PartOfSpeech = uw.Word.PartOfSpeech,
                Example = uw.Word.Example,
                DifficultyLevel = uw.Word.DifficultyLevel,
                AddedAt = uw.SavedAt,
                CategoryName = uw.Word.Categories?.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault() ?? "General",
                Progress = progressDict.TryGetValue(uw.Word.Id, out var points) ? points : 0
            }).ToList();

            logger.LogInformation("Успішно отримано {Count} слів для користувача {UserId}", words.Count, userId);

            return words;
        }

        public async Task<Result> DeleteWordAsync(string userId, int wordId)
        {
            var userWord = await wordRepository.GetUserWordAsync(userId, wordId);

            if (userWord == null)
            {
                logger.LogWarning("Спроба видалити слово {WordId} користувачем {UserId}, але воно не знайдено.", wordId, userId);
                return "Word not found in your saved words.";
            }

            var word = userWord.Word;
            if (word == null)
            {
                await wordRepository.DeleteUserWordAsync(userWord);
                return true;
            }

            if (!word.IsGlobal && word.CreatorId == userId)
            {
                await wordRepository.DeleteWordAsync(word);
                logger.LogInformation("Користувач {UserId} як творець видалив персональне слово {WordId}.", userId, wordId);
                return true;
            }

            await wordRepository.DeleteUserWordAsync(userWord);
            logger.LogInformation("Користувач {UserId} видалив слово {WordId} зі збережених.", userId, wordId);
            return true;
        }

        public async Task<PagedResult<PersonalWordDTO>> GetPersonalWordsAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize)
        {
            logger.LogInformation("Отримання сторінки слів для {UserId}: page={Page}, pageSize={PageSize}", userId, page, pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = _pagination.DefaultPersonalWordsPageSize;
            }

            var (itemsUw, total) = await wordRepository.GetPersonalWordsPageAsync(userId, searchTerm, category, partOfSpeech, difficultyLevel, page, pageSize);

            var wordIds = itemsUw.Select(uw => uw.WordId).ToList();
            var progresses = await wordProgressRepository.GetWordProgressesAsync(userId, wordIds);
            var progressDict = progresses.ToDictionary(p => p.WordId, p => p.Points);

            var items = itemsUw.Select(uw => new PersonalWordDTO
            {
                Id = uw.Word.Id,
                Term = uw.Word.Term,
                Translation = uw.Word.Translation,
                Transcription = uw.Word.Transcription,
                Meaning = uw.Word.Meaning,
                PartOfSpeech = uw.Word.PartOfSpeech,
                Example = uw.Word.Example,
                DifficultyLevel = uw.Word.DifficultyLevel,
                AddedAt = uw.SavedAt,
                CategoryName = uw.Word.Categories?.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault() ?? "General",
                Progress = progressDict.TryGetValue(uw.Word.Id, out var points) ? points : 0
            }).ToList();

            return new PagedResult<PersonalWordDTO>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<PagedResult<SystemWordDTO>> GetSystemWordsAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize)
        {
            logger.LogInformation("Getting system words page for {UserId}: page={Page}, pageSize={PageSize}", userId, page, pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = _pagination.DefaultSystemWordsPageSize;
            }

            var (itemsResult, total) = await wordRepository.GetSystemWordsPageAsync(userId, searchTerm, category, partOfSpeech, difficultyLevel, page, pageSize);

            var items = itemsResult.Select(result => new SystemWordDTO
            {
                Id = result.word.Id,
                Term = result.word.Term,
                Translation = result.word.Translation,
                Transcription = result.word.Transcription,
                Meaning = result.word.Meaning,
                PartOfSpeech = result.word.PartOfSpeech,
                Example = result.word.Example,
                DifficultyLevel = result.word.DifficultyLevel,
                CategoryName = result.word.Categories?.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault() ?? "General",
                IsSaved = result.isSaved
            }).ToList();

            return new PagedResult<SystemWordDTO>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<Result> SaveSystemWordAsync(string userId, int wordId)
        {
            var word = await wordRepository.GetWordAsync(wordId);

            if (word == null || !word.IsGlobal)
            {
                return "Word not found or is not a system word.";
            }

            var alreadySaved = await wordRepository.UserHasWordAsync(userId, wordId);
            if (alreadySaved)
            {
                return "You have already saved this word.";
            }

            var userWord = new UserWord
            {
                UserId = userId,
                WordId = wordId,
                SavedAt = DateTime.UtcNow
            };

            await wordRepository.SaveUserWordAsync(userWord);

            logger.LogInformation("User {UserId} saved global word {WordId}.", userId, wordId);

            return true;
        }

        public async Task<Result> SaveWordToLibraryAsync(string userId, int wordId)
        {
            if (wordId <= 0)
            {
                logger.LogWarning("User {UserId} submitted an invalid word id {WordId} for saving.", userId, wordId);
                return "Word ID must be greater than zero.";
            }

            var word = await wordRepository.GetWordAsync(wordId);
            if (word == null)
            {
                logger.LogWarning("User {UserId} attempted to save a missing word {WordId}.", userId, wordId);
                return "Word not found.";
            }

            var alreadySaved = await wordRepository.UserHasWordAsync(userId, wordId);
            if (alreadySaved)
            {
                logger.LogInformation("User {UserId} attempted to save a duplicate word {WordId}.", userId, wordId);
                return "You have already saved this word.";
            }

            var userWord = new UserWord
            {
                UserId = userId,
                WordId = wordId,
                SavedAt = DateTime.UtcNow
            };

            await wordRepository.SaveUserWordAsync(userWord);

            logger.LogInformation("User {UserId} saved word {WordId} to their library.", userId, wordId);

            return true;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await cache.GetOrCreateAsync(CacheKeys.AllCategories, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheSettings.CategoriesCacheDurationMinutes);
                return await categoryRepository.GetAllCategoriesAsync();
            }) ?? [];
        }

        public async Task<IEnumerable<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids)
        {
            return await categoryRepository.GetCategoriesByIdsAsync(ids);
        }

        public async Task<Category> CreateCategoryAsync(string name)
        {
            var existing = await categoryRepository.GetCategoryByNameAsync(name);
            if (existing != null)
            {
                return existing;
            }

            var cat = new Category { Name = name.Trim() };
            var result = await categoryRepository.CreateCategoryAsync(cat);

            cache.Remove(CacheKeys.AllCategories);

            return result;
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await categoryRepository.GetCategoryByNameAsync(name);
        }

        public async Task<Result<Word>> GetPersonalWordForEditAsync(string userId, int wordId)
        {
            var userWord = await wordRepository.GetUserWordAsync(userId, wordId);

            if (userWord?.Word == null)
            {
                logger.LogWarning("Користувач {UserId} намагався отримати доступ до неіснуючого слова {WordId}", userId, wordId);
                return "Word not found or access denied.";
            }

            return userWord.Word;
        }

        public async Task<Result> UpdateUserWordAsync(string userId, UpdateWordDTO dto)
        {
            var userWord = await wordRepository.GetUserWordAsync(userId, dto.WordId);

            if (userWord == null)
            {
                logger.LogWarning("Користувач {UserId} намагався відредагувати неіснуюче або чуже слово {WordId}", userId, dto.WordId);
                return "Word not found or you don't have permission to edit it.";
            }

            var word = userWord.Word;
            word.Term = dto.Term;
            word.Translation = dto.Translation;
            word.Meaning = dto.Meaning;
            word.Example = dto.Example;

            await wordRepository.UpdateWordAsync(word);
            logger.LogInformation("Слово {WordId} успішно оновлено користувачем {UserId}", dto.WordId, userId);

            return true;
        }

        public async Task<Result> CreateSystemWordAsync(CreateSystemWordDTO dto)
        {
            var word = new Word
            {
                Term = dto.Term,
                Translation = dto.Translation,
                Transcription = dto.Transcription,
                Meaning = dto.Meaning,
                PartOfSpeech = dto.PartOfSpeech,
                Example = dto.Example,
                DifficultyLevel = dto.DifficultyLevel,
                IsGlobal = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var cats = await categoryRepository.GetCategoriesByIdsAsync(dto.CategoryIds);
                foreach (var cat in cats)
                {
                    word.Categories.Add(cat);
                }
            }

            await wordRepository.CreateWordAsync(word);
            logger.LogInformation("Адміністратор створив системне слово {WordId}", word.Id);
            return true;
        }

        public async Task<Result> UpdateSystemWordAsync(int id, UpdateSystemWordDTO dto)
        {
            var word = await wordRepository.GetWordAsync(id);
            if (word == null || !word.IsGlobal)
            {
                return "System word not found.";
            }

            word.Term = dto.Term;
            word.Translation = dto.Translation;
            word.Transcription = dto.Transcription;
            word.Meaning = dto.Meaning;
            word.PartOfSpeech = dto.PartOfSpeech;
            word.Example = dto.Example;
            word.DifficultyLevel = dto.DifficultyLevel;
            word.UpdatedAt = DateTime.UtcNow;

            word.Categories.Clear();
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var cats = await categoryRepository.GetCategoriesByIdsAsync(dto.CategoryIds);
                foreach (var cat in cats)
                {
                    word.Categories.Add(cat);
                }
            }

            await wordRepository.UpdateWordAsync(word);
            logger.LogInformation("Адміністратор оновив системне слово {WordId}", id);
            return true;
        }

        public async Task<Result> DeleteSystemWordAsync(int id)
        {
            var word = await wordRepository.GetWordAsync(id);
            if (word == null || !word.IsGlobal)
            {
                return "System word not found.";
            }

            await wordRepository.DeleteWordAsync(word);
            logger.LogInformation("Адміністратор видалив системне слово {WordId}", id);
            return true;
        }

        public async Task<Result<Word>> GetSystemWordForEditAsync(int id)
        {
            var word = await wordRepository.GetWordAsync(id);
            if (word == null || !word.IsGlobal)
            {
                return "Global word not found.";
            }

            return word;
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync()
        {
            return await wordRepository.GetAllWordsAsync();
        }
    }
}
