using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> WordExistsForUserAsync(string userId, string termLower);

        Task<Word> CreatePersonalWordAsync(Word word, UserWord userWord);

        Task<Word> CreateWordAsync(Word word);

        Task<UserWord?> GetUserWordAsync(string userId, int wordId);

        Task DeleteUserWordAsync(UserWord userWord);

        Task DeleteWordAsync(Word word);

        Task<IEnumerable<UserWord>> GetPersonalWordsWithDetailsAsync(string userId);

        Task<(IEnumerable<UserWord> Items, int Total)> GetPersonalWordsPageAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize);

        Task<(IEnumerable<(Word word, bool isSaved)> Items, int Total)> GetSystemWordsPageAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize);

        Task<Word?> GetWordAsync(int wordId);

        Task<bool> UserHasWordAsync(string userId, int wordId);

        Task SaveUserWordAsync(UserWord userWord);

        Task UpdateWordAsync(Word word);

        Task<IEnumerable<Word>> GetAllWordsAsync();
    }
}