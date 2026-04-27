using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class QuizService(
        IWordRepository wordRepository,
        ITrainingSessionRepository trainingSessionRepository,
        ILogger<QuizService> logger) : IQuizService
    {
        public async Task<Result<StudySessionDTO>> StartCustomQuizAsync(string userId, QuizSetupDTO setup)
        {
            try
            {
                var words = await wordRepository.GetRandomPersonalWordsByCriteriaAsync(
                    userId,
                    setup.Category,
                    setup.PartOfSpeech,
                    setup.MinDifficulty != null ? GetLevelFromIndex(setup.MinDifficulty.Value) : null,
                    setup.MaxDifficulty != null ? GetLevelFromIndex(setup.MaxDifficulty.Value) : null,
                    setup.WordCount);

                var wordList = words.ToList();

                if (!wordList.Any())
                {
                    logger.LogWarning("User {UserId} tried to start a quiz, but no words were found matching the criteria.", userId);
                    return Result<StudySessionDTO>.Failure("No words found matching the specified criteria.");
                }

                var session = new TrainingSession
                {
                    UserId = userId,
                    BundleId = null,
                    StartedAt = DateTime.UtcNow,
                    TotalCards = wordList.Count,
                    KnownCount = 0,
                    UnknownCount = 0
                };

                var createdSession = await trainingSessionRepository.CreateSessionAsync(session);

                var cards = wordList.Select(w => new StudyCardDTO
                {
                    WordId = w.Id,
                    Term = w.Term,
                    Translation = w.Translation,
                    Transcription = w.Transcription,
                    Meaning = w.Meaning,
                    PartOfSpeech = w.PartOfSpeech,
                    Example = w.Example,
                    ImageUrl = w.ImageUrl,
                    DifficultyLevel = w.DifficultyLevel
                }).ToList();

                var dto = new StudySessionDTO
                {
                    SessionId = createdSession.Id,
                    BundleId = 0,
                    BundleTitle = "Custom Quiz",
                    Cards = cards,
                    TotalCards = cards.Count,
                    StartedAt = createdSession.StartedAt
                };

                logger.LogInformation(
                    "User {UserId} started a custom quiz {SessionId} ({CardCount} cards).",
                    userId,
                    createdSession.Id,
                    cards.Count);

                return Result<StudySessionDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating custom quiz for user {UserId}.", userId);
                return Result<StudySessionDTO>.Failure("Error creating quiz.");
            }
        }

        public async Task<Result<IEnumerable<string>>> GetAvailableCategoriesAsync(string userId)
        {
            try
            {
                var personalWords = await wordRepository.GetPersonalWordsWithDetailsAsync(userId);
                var categories = personalWords
                    .SelectMany(uw => uw.Word.Categories)
                    .Select(c => c.Name)
                    .Distinct()
                    .OrderBy(c => c);

                return Result<IEnumerable<string>>.Success(categories);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving categories for user {UserId}.", userId);
                return Result<IEnumerable<string>>.Failure("Error retrieving categories.");
            }
        }

        public async Task<Result<IEnumerable<string>>> GetAvailablePartsOfSpeechAsync(string userId)
        {
            try
            {
                var personalWords = await wordRepository.GetPersonalWordsWithDetailsAsync(userId);
                var parts = personalWords
                    .Select(uw => uw.Word.PartOfSpeech)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Cast<string>()
                    .Distinct()
                    .OrderBy(p => p);

                return Result<IEnumerable<string>>.Success(parts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving parts of speech for user {UserId}.", userId);
                return Result<IEnumerable<string>>.Failure("Error retrieving parts of speech.");
            }
        }

        private static string GetLevelFromIndex(int index)
        {
            var levels = new[] { "A1", "A2", "B1", "B2", "C1", "C2" };
            if (index < 0 || index >= levels.Length)
            {
                return levels[0];
            }

            return levels[index];
        }
    }
}
