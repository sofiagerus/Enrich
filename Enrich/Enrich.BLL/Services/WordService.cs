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
    }
}