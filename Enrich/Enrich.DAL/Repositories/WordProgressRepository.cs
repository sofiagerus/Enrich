using Enrich.DAL.Data;
using Enrich.DAL.Entities;
using Enrich.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enrich.DAL.Repositories
{
    public class WordProgressRepository(ApplicationDbContext dbContext) : IWordProgressRepository
    {
        public async Task<WordProgress?> GetWordProgressAsync(string userId, int wordId)
        {
            return await dbContext.WordProgresses
                .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.WordId == wordId);
        }

        public async Task<WordProgress> CreateWordProgressAsync(WordProgress progress)
        {
            dbContext.WordProgresses.Add(progress);
            await dbContext.SaveChangesAsync();
            return progress;
        }

        public async Task UpdateWordProgressAsync(WordProgress progress)
        {
            dbContext.WordProgresses.Update(progress);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<WordProgress>> GetUserWordProgressAsync(string userId)
        {
            return await dbContext.WordProgresses
                .Where(wp => wp.UserId == userId)
                .Include(wp => wp.Word)
                .ToListAsync();
        }
    }
}
