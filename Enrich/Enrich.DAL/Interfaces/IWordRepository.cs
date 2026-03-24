using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> WordExistsForUserAsync(string userId, string termLower);

        Task<Word> CreatePersonalWordAsync(string userId, string term, string? translation, string? transcription,
            string? meaning, string? partOfSpeech, string? example, string? difficultyLevel);

        Task<IEnumerable<UserWord>> GetPersonalWordsWithDetailsAsync(string userId);
    }
}
