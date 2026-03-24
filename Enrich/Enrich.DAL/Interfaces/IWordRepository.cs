using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> WordExistsForUserAsync(string userId, string termLower);

        Task<Word> CreatePersonalWordAsync(Word word, UserWord userWord);

        Task<IEnumerable<UserWord>> GetPersonalWordsWithDetailsAsync(string userId);
    }
}
