using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface ITrainingSessionRepository
    {
        Task<TrainingSession> CreateSessionAsync(TrainingSession session);

        Task<TrainingSession?> GetSessionByIdAsync(int sessionId);

        Task<TrainingSession?> GetSessionByIdWithDetailsAsync(int sessionId);

        Task UpdateSessionAsync(TrainingSession session);

        Task<IEnumerable<TrainingSession>> GetUserSessionsAsync(string userId, int bundleId);

        Task AddSessionResultAsync(SessionResult result);

        Task<SessionResult?> GetSessionResultAsync(int sessionId, int wordId);
    }
}
