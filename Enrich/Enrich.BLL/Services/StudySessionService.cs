using Enrich.BLL.Common;
using Enrich.BLL.DTOs;
using Enrich.BLL.Interfaces;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enrich.BLL.Services
{
    public class StudySessionService(
        ITrainingSessionRepository trainingSessionRepository,
        IWordProgressRepository wordProgressRepository,
        IBundleRepository bundleRepository,
        IWordRepository wordRepository,
        ILogger<StudySessionService> logger) : IStudySessionService
    {
        private const int PointsForKnown = 10;
        private const int PointsForUnknown = 5;
        private const int MaxPoints = 100;

        public async Task<Result<StudySessionDTO>> StartStudySessionAsync(string userId, int bundleId)
        {
            try
            {
                var bundle = await bundleRepository.GetBundleByIdWithDetailsAsync(bundleId);

                if (bundle == null)
                {
                    logger.LogWarning("Користувач {UserId} спробував розпочати сесію для неіснуючого бандлу {BundleId}.", userId, bundleId);
                    return Result<StudySessionDTO>.Failure("Бандл не знайдено.");
                }

                if (!bundle.Words.Any())
                {
                    logger.LogWarning("Користувач {UserId} спробував розпочати сесію для порожного бандлу {BundleId}.", userId, bundleId);
                    return Result<StudySessionDTO>.Failure("Бандл не містить слів для вивчення.");
                }

                var session = new TrainingSession
                {
                    UserId = userId,
                    BundleId = bundleId,
                    StartedAt = DateTime.UtcNow,
                    TotalCards = bundle.Words.Count,
                    KnownCount = 0,
                    UnknownCount = 0
                };

                var createdSession = await trainingSessionRepository.CreateSessionAsync(session);

                var cards = bundle.Words.Select(w => new StudyCardDTO
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
                    BundleId = bundleId,
                    BundleTitle = bundle.Title,
                    Cards = cards,
                    TotalCards = cards.Count,
                    StartedAt = createdSession.StartedAt
                };

                logger.LogInformation(
                    "Користувач {UserId} розпочав сесію {SessionId} для бандлу {BundleId} ({CardCount} карток).",
                    userId,
                    createdSession.Id,
                    bundleId,
                    cards.Count);

                return Result<StudySessionDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при розпочатті сесії вивчення для користувача {UserId} бандлу {BundleId}.",
                    userId,
                    bundleId);

                return Result<StudySessionDTO>.Failure("Помилка при розпочатті сесії.");
            }
        }

        public async Task<Result<StudyProgressDTO>> SubmitAnswerAsync(string userId, SubmitAnswerDTO dto)
        {
            try
            {
                var session = await trainingSessionRepository.GetSessionByIdAsync(dto.SessionId);

                if (session == null)
                {
                    logger.LogWarning("User {UserId} tried to submit an answer for a non-existent session {SessionId}.", userId, dto.SessionId);
                    return Result<StudyProgressDTO>.Failure("Session not found.");
                }

                if (session.UserId != userId)
                {
                    logger.LogWarning("User {UserId} tried to submit an answer for another user's session {SessionId}.", userId, dto.SessionId);
                    return Result<StudyProgressDTO>.Failure("Access denied.");
                }

                if (session.FinishedAt.HasValue)
                {
                    logger.LogWarning("User {UserId} tried to submit an answer for a finished session {SessionId}.", userId, dto.SessionId);
                    return Result<StudyProgressDTO>.Failure("Session is already finished.");
                }

                var existingResult = await trainingSessionRepository.GetSessionResultAsync(dto.SessionId, dto.WordId);

                if (existingResult != null)
                {
                    logger.LogWarning("User {UserId} tried to submit a duplicate answer for word {WordId} in session {SessionId}.", userId, dto.WordId, dto.SessionId);
                    return Result<StudyProgressDTO>.Failure("Answer for this word has already been submitted.");
                }

                var word = await wordRepository.GetWordAsync(dto.WordId);

                if (word == null)
                {
                    logger.LogWarning("User {UserId} tried to submit an answer for a non-existent word {WordId}.", userId, dto.WordId);
                    return Result<StudyProgressDTO>.Failure("Word not found.");
                }

                int pointsAwarded = dto.IsKnown ? PointsForKnown : PointsForUnknown;

                var sessionResult = new SessionResult
                {
                    SessionId = dto.SessionId,
                    WordId = dto.WordId,
                    IsKnown = dto.IsKnown,
                    PointsAwarded = pointsAwarded
                };

                await trainingSessionRepository.AddSessionResultAsync(sessionResult);

                var wordProgress = await wordProgressRepository.GetWordProgressAsync(userId, dto.WordId);

                if (wordProgress == null)
                {
                    wordProgress = new WordProgress
                    {
                        UserId = userId,
                        WordId = dto.WordId,
                        Points = pointsAwarded,
                        IsLearned = pointsAwarded >= MaxPoints,
                        LastReviewedAt = DateTime.UtcNow
                    };

                    await wordProgressRepository.CreateWordProgressAsync(wordProgress);
                }
                else
                {
                    wordProgress.Points = Math.Min(wordProgress.Points + pointsAwarded, MaxPoints);
                    wordProgress.IsLearned = wordProgress.Points >= MaxPoints;
                    wordProgress.LastReviewedAt = DateTime.UtcNow;

                    await wordProgressRepository.UpdateWordProgressAsync(wordProgress);
                }

                if (dto.IsKnown)
                {
                    session.KnownCount++;
                }
                else
                {
                    session.UnknownCount++;
                }

                await trainingSessionRepository.UpdateSessionAsync(session);

                logger.LogInformation(
                    "User {UserId} submitted answer '{Answer}' for word {WordId} in session {SessionId}. Points awarded: {Points}.",
                    userId,
                    dto.IsKnown ? "known" : "unknown",
                    dto.WordId,
                    dto.SessionId,
                    pointsAwarded);

                var progress = new StudyProgressDTO
                {
                    SessionId = dto.SessionId,
                    TotalCards = session.TotalCards,
                    KnownCount = session.KnownCount,
                    UnknownCount = session.UnknownCount,
                    TotalPoints = (session.KnownCount * PointsForKnown) + (session.UnknownCount * PointsForUnknown)
                };

                return Result<StudyProgressDTO>.Success(progress);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error submitting answer by user {UserId} for session {SessionId}.",
                    userId,
                    dto.SessionId);

                return Result<StudyProgressDTO>.Failure("Error processing answer.");
            }
        }

        public async Task<Result> FinishStudySessionAsync(string userId, int sessionId)
        {
            try
            {
                var session = await trainingSessionRepository.GetSessionByIdAsync(sessionId);

                if (session == null)
                {
                    logger.LogWarning("User {UserId} tried to finish a non-existent session {SessionId}.", userId, sessionId);
                    return Result.Failure("Session not found.");
                }

                if (session.UserId != userId)
                {
                    logger.LogWarning("User {UserId} tried to finish another user's session {SessionId}.", userId, sessionId);
                    return Result.Failure("Access denied.");
                }

                session.FinishedAt = DateTime.UtcNow;
                await trainingSessionRepository.UpdateSessionAsync(session);

                logger.LogInformation(
                    "User {UserId} finished session {SessionId}. Result: {Known} known, {Unknown} unknown out of {Total} cards.",
                    userId,
                    sessionId,
                    session.KnownCount,
                    session.UnknownCount,
                    session.TotalCards);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при завершенні сесії {SessionId} користувачем {UserId}.",
                    sessionId,
                    userId);

                return Result.Failure("Помилка при завершенні сесії.");
            }
        }

        public async Task<Result<StudyProgressDTO>> GetSessionProgressAsync(string userId, int sessionId)
        {
            try
            {
                var session = await trainingSessionRepository.GetSessionByIdWithDetailsAsync(sessionId);

                if (session == null)
                {
                    return Result<StudyProgressDTO>.Failure("Сесія не знайдена.");
                }

                if (session.UserId != userId)
                {
                    return Result<StudyProgressDTO>.Failure("Доступ заборонено.");
                }

                var totalPoints = session.SessionResults?.Sum(sr => sr.PointsAwarded) ?? 0;

                var progress = new StudyProgressDTO
                {
                    SessionId = sessionId,
                    TotalCards = session.TotalCards,
                    KnownCount = session.KnownCount,
                    UnknownCount = session.UnknownCount,
                    TotalPoints = totalPoints
                };

                return Result<StudyProgressDTO>.Success(progress);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Помилка при отриманні прогресу сесії {SessionId} користувачем {UserId}.",
                    sessionId,
                    userId);

                return Result<StudyProgressDTO>.Failure("Помилка при отриманні прогресу.");
            }
        }
    }
}
