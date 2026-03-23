using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class WordService(
        ApplicationDbContext dbContext,
        ILogger<WordService> logger) : IWordService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreatePersonalWordAsync(string userId, CreatePersonalWordDTO dto)
        {
            var termLower = dto.Term.Trim().ToLowerInvariant();

            var duplicateExists = await dbContext.Words
                .AnyAsync(w =>
                    w.CreatorId == userId &&
                    w.Term.ToLower() == termLower);

            if (duplicateExists)
            {
                logger.LogWarning(
                    "Користувач {UserId} спробував створити дублікат слова: '{Term}'.",
                    userId,
                    dto.Term);

                return (false, $"У вашому особистому словнику вже є слово '{dto.Term}'.");
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };

            dbContext.Words.Add(word);
            await dbContext.SaveChangesAsync();

            var userWord = new UserWord
            {
                UserId = userId,
                WordId = word.Id,
                SavedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };

            dbContext.UserWords.Add(userWord);
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Користувач {UserId} успішно створив нове персональне слово '{Term}' (ID слова: {WordId}).",
                userId,
                word.Term,
                word.Id);

            return (true, null);
        }

        public async Task<IEnumerable<PersonalWordDTO>> GetPersonalWordsAsync(string userId)
        {
            logger.LogInformation("Отримання списку персональних слів для користувача {UserId}", userId);

            var userWordCount = await dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .CountAsync();

            logger.LogInformation("У користувача {UserId} знайдено {Count} записів у таблиці UserWords", userId, userWordCount);

            var words = await dbContext.UserWords
                .Where(uw => uw.UserId == userId)
                .Include(uw => uw.Word)
                .Select(uw => new PersonalWordDTO
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
                })
                .ToListAsync();

            logger.LogInformation("Успішно отримано {Count} слів для користувача {UserId}", words.Count, userId);

            return words;
        }
    }
}