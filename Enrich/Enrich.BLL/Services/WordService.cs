using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class WordService(
        IWordRepository wordRepository,
        ILogger<WordService> logger) : IWordService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreatePersonalWordAsync(string userId, CreatePersonalWordDTO dto)
        {
            var termLower = dto.Term.Trim().ToLowerInvariant();

            var duplicateExists = await wordRepository.WordExistsForUserAsync(userId, termLower);

            if (duplicateExists)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував створити дублікат слова: '{Term}'.",
                    userId,
                    dto.Term);

                return (false, $"У вашому особистому словнику вже є слово '{dto.Term}'.");
            }

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var categories = new List<Category>();
            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var fetchedCategories = await wordRepository.GetCategoriesByIdsAsync(dto.CategoryIds);
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

            return (true, null);
        }

        public async Task<IEnumerable<PersonalWordDTO>> GetPersonalWordsAsync(string userId)
        {
            logger.LogInformation("Отримання списку персональних слів для користувача {UserId}", userId);

            var userWords = await wordRepository.GetPersonalWordsWithDetailsAsync(userId);

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
                CategoryName = uw.Word.Categories?.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault() ?? "General"
            }).ToList();

            logger.LogInformation("Успішно отримано {Count} слів для користувача {UserId}", words.Count, userId);

            return words;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteWordAsync(string userId, int wordId)
        {
            var userWord = await wordRepository.GetUserWordAsync(userId, wordId);

            if (userWord == null)
            {
                logger.LogWarning("Спроба видалити слово {WordId} користувачем {UserId}, але воно не знайдено.", wordId, userId);
                return (false, "Word not found in your saved words.");
            }

            var word = userWord.Word;
            if (word == null)
            {
                await wordRepository.DeleteUserWordAsync(userWord);
                return (true, null);
            }

            if (!word.IsGlobal && word.CreatorId == userId)
            {
                await wordRepository.DeleteWordAsync(word);
                logger.LogInformation("Користувач {UserId} як творець видалив персональне слово {WordId}.", userId, wordId);
                return (true, null);
            }

            await wordRepository.DeleteUserWordAsync(userWord);
            logger.LogInformation("Користувач {UserId} видалив слово {WordId} зі збережених.", userId, wordId);
            return (true, null);
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
                pageSize = 20;
            }

            var (itemsUw, total) = await wordRepository.GetPersonalWordsPageAsync(userId, searchTerm, category, partOfSpeech, difficultyLevel, page, pageSize);

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
                CategoryName = uw.Word.Categories?.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault() ?? "General"
            }).ToList();

            return new PagedResult<PersonalWordDTO>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await wordRepository.GetAllCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids)
        {
            return await wordRepository.GetCategoriesByIdsAsync(ids);
        }

        public async Task<Category> CreateCategoryAsync(string name)
        {
            var existing = await wordRepository.GetCategoryByNameAsync(name);
            if (existing != null)
            {
                return existing;
            }

            var cat = new Category { Name = name.Trim() };
            return await wordRepository.CreateCategoryAsync(cat);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await wordRepository.GetCategoryByNameAsync(name);
        }
    }
}