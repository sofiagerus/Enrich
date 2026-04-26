using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enrich.DAL.Repositories
{
    public class TrainingSessionRepository(ApplicationDbContext dbContext) : ITrainingSessionRepository
    {
        public async Task<TrainingSession> CreateSessionAsync(TrainingSession session)
        {
            dbContext.TrainingSessions.Add(session);
            await dbContext.SaveChangesAsync();
            return session;
        }

        public async Task<TrainingSession?> GetSessionByIdAsync(int sessionId)
        {
            return await dbContext.TrainingSessions.FindAsync(sessionId);
        }

        public async Task<TrainingSession?> GetSessionByIdWithDetailsAsync(int sessionId)
        {
            return await dbContext.TrainingSessions
                .Include(s => s.Bundle)
                .ThenInclude(b => b!.Words)
                .Include(s => s.SessionResults)
                .ThenInclude(sr => sr.Word)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task UpdateSessionAsync(TrainingSession session)
        {
            dbContext.TrainingSessions.Update(session);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<TrainingSession>> GetUserSessionsAsync(string userId, int bundleId)
        {
            return await dbContext.TrainingSessions
                .Where(s => s.UserId == userId && s.BundleId == bundleId)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync();
        }

        public async Task AddSessionResultAsync(SessionResult result)
        {
            dbContext.SessionResults.Add(result);
            await dbContext.SaveChangesAsync();
        }

        public async Task<SessionResult?> GetSessionResultAsync(int sessionId, int wordId)
        {
            return await dbContext.SessionResults
                .FirstOrDefaultAsync(sr => sr.SessionId == sessionId && sr.WordId == wordId);
        }
    }
}
