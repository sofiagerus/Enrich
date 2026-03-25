using Enrich.DAL.Entities;

namespace Enrich.DAL.Interfaces
{
    public interface IWordRepository
    {
        Task<bool> WordExistsForUserAsync(string userId, string termLower);

        Task<Word> CreatePersonalWordAsync(Word word, UserWord userWord);

        Task<UserWord?> GetUserWordAsync(string userId, int wordId);

        Task DeleteUserWordAsync(UserWord userWord);

        Task DeleteWordAsync(Word word);

        Task<IEnumerable<UserWord>> GetPersonalWordsWithDetailsAsync(string userId);

        IQueryable<UserWord> QueryPersonalWords(string userId);

        Task<(IEnumerable<UserWord> Items, int Total)> GetPersonalWordsPageAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize);

        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<IEnumerable<Category>> GetCategoriesByIdsAsync(IEnumerable<int> ids);

        Task<Category> CreateCategoryAsync(Category category);

        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
