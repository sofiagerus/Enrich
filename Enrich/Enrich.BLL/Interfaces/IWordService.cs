using Enrich.BLL.DTOs;

namespace Enrich.BLL.Interfaces
{
    public interface IWordService
    {
        Task<(bool Success, string? ErrorMessage)> CreatePersonalWordAsync(string userId, CreatePersonalWordDTO dto);

        Task<(bool Success, string? ErrorMessage)> DeleteWordAsync(string userId, int wordId);

        Task<IEnumerable<PersonalWordDTO>> GetPersonalWordsAsync(string userId);

        Task<PagedResult<PersonalWordDTO>> GetPersonalWordsAsync(string userId, string? searchTerm, string? category, string? partOfSpeech, string? difficultyLevel, int page, int pageSize);
    }
}
