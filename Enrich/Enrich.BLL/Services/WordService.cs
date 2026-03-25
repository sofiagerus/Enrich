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
            };

            var userWord = new UserWord
            {
                UserId = userId,
                SavedAt = now,
            };

            var createdWord = await wordRepository.CreatePersonalWordAsync(word, userWord);

            logger.LogInformation(
                "Користувач {UserId} успішно створив нове персональне слово '{Term}' (ID слова: {WordId}).",
                userId,
                createdWord.Term,
                createdWord.Id);

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
            }).ToList();

            logger.LogInformation("Успішно отримано {Count} слів для користувача {UserId}", words.Count, userId);

            return words;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteWordAsync(string userId, int wordId)
        {
            // Find the UserWord relation (includes Word)
            var userWord = await wordRepository.GetUserWordAsync(userId, wordId);

            if (userWord == null)
            {
                logger.LogWarning("Спроба видалити слово {WordId} користувачем {UserId}, але воно не знайдено у його збережених словах.", wordId, userId);
                return (false, "Word not found in your saved words.");
            }

            var word = userWord.Word;
            if (word == null)
            {
                // Defensive: should not happen
                await wordRepository.DeleteUserWordAsync(userWord);
                logger.LogInformation("Користувач {UserId} видалив зв'язок до слова {WordId}, але слово відсутнє в базі.", userId, wordId);
                return (true, null);
            }

            // If the user is the creator of the word and it's not global, delete the word entirely.
            if (!word.IsGlobal && word.CreatorId == userId)
            {
                await wordRepository.DeleteWordAsync(word);
                logger.LogInformation("Користувач {UserId} як творець видалив персональне слово {WordId}.", userId, wordId);
                return (true, null);
            }

            // Otherwise, just remove the UserWord relation (user unsaves the system word)
            await wordRepository.DeleteUserWordAsync(userWord);
            logger.LogInformation("Користувач {UserId} видалив слово {WordId} зі своїх збережених слів.", userId, wordId);
            return (true, null);
        }

        public async Task<PagedResult<PersonalWordDTO>> GetPersonalWordsAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize)
        {
            logger.LogInformation("Отримання сторінки персональних слів для користувача {UserId}: page={Page}, pageSize={PageSize}, search={Search}, pos={Pos}, level={Level}", userId, page, pageSize, searchTerm, partOfSpeech, difficultyLevel);

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
            }).ToList();

            return new PagedResult<PersonalWordDTO>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            };
        }
    }
}