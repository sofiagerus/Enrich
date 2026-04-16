using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IWordProgressRepository
    {
        Task<WordProgress?> GetWordProgressAsync(string userId, int wordId);

        Task<WordProgress> CreateWordProgressAsync(WordProgress progress);

        Task UpdateWordProgressAsync(WordProgress progress);

        Task<IEnumerable<WordProgress>> GetUserWordProgressAsync(string userId);
    }
}
